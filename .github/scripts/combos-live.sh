#!/usr/bin/env bash
# combo-maker quoting the request combos/ opens, end to end, for live.yml.
#
#   .github/scripts/combos-live.sh <name>
#
# combo-maker runs as the maker account ($MAKER_KEY_ID, $MAKER_KEY_PEM) while
# combos/, as the account in STX_KEY_ID / STX_PRIVATE_KEY, requests a small
# combo, takes the best quote and cashes it out. Both are built in Release.
#
# Checked: combo-maker quoted the request and its quote was the one placed
# (`[won]` for that request: the placed push reaches only the winning maker),
# and combos/ bought, sold and holds nothing at the end. combo-maker quotes only
# this player's requests (--player). Another maker can still quote higher, so a
# buy placed elsewhere is tried once more with a new request. Winning the cash out too is reported, not
# required.
set -eo pipefail
# Job control, so combo-maker in the background still takes SIGINT and stops cleanly.
set -m
name=$1
log=maker.txt

# The maker quotes only this run's player, so it never trades with another run.
player=$(dotnet combos/bin/Release/net10.0/Combos.dll --player-id | awk '/^Player:/ { print $2 }')
[ -n "$player" ] || { echo "::error::combos printed no player id on $name"; exit 1; }
STX_KEY_ID="$MAKER_KEY_ID" STX_PRIVATE_KEY="$MAKER_KEY_PEM" \
  dotnet combo-maker/bin/Release/net10.0/ComboMaker.dll --player "$player" \
  --seconds 300 --win-multiple 12 --cash-out-price 0.40 --quote-seconds 30 > "$log" 2>&1 &
pid=$!
for _ in $(seq 1 60); do grep -q '^Quoting combos on ' "$log" && break; sleep 0.5; done
grep -q '^Quoting combos on ' "$log" || { cat "$log"; echo "::error::combo-maker did not join combo_quote on $name"; exit 1; }

status=0
for attempt in 1 2; do
  req="combos-$attempt.txt"
  status=0
  dotnet combos/bin/Release/net10.0/Combos.dll | tee "$req" || status=$?
  sleep 2
  buy=$(awk '/^Request:/ { print substr($2, 1, 8) }' "$req")
  if [ "$attempt" = 1 ] && [ "$status" = 0 ] && [ -n "$buy" ] && ! grep -q "^\[won\]      $buy " "$log"; then
    echo "::warning::another maker's quote was placed on $buy on $name; trying once more"
    continue
  fi
  break
done
kill -INT "$pid" 2>/dev/null || true
wait "$pid" || true
echo "--- combo-maker"; cat "$log"

sell=$(awk '/^Cash out:/ { print substr($3, 1, 8) }' "$req")
[ -n "$buy" ] || { echo "::error::combos opened no request on $name"; exit 1; }
grep -q "^\[quoted\]   $buy " "$log" || { echo "::error::combo-maker did not quote request $buy on $name"; exit 1; }
grep -q '^Bought: ' "$req" || { echo "::error::combos bought nothing on $name"; exit 1; }
grep -q "^\[won\]      $buy " "$log" || { echo "::error::combo-maker quoted $buy but another maker's quote was placed on $name"; exit 1; }
grep -q '^Sold: ' "$req" || { echo "::error::combos did not cash out on $name"; exit 1; }
grep -Eq '^Final:   position 0(\.0+)? ' "$req" || { echo "::error::combos left a position on $name"; exit 1; }
grep -q '^Stopped\.' "$log" || { echo "::error::combo-maker did not stop cleanly on $name"; exit 1; }
if [ -n "$sell" ] && ! grep -q "^\[won\]      $sell " "$log"; then
  echo "::warning::another maker took the cash out $sell on $name; combo-maker still holds its side of the combo"
fi
exit "$status"
