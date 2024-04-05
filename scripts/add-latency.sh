#!/usr/bin/env bash

if [ "$#" != 1 ]; then
  echo "usage: $0 <latency in ms>" >&2 && exit 1
fi

sudo tc qdisc del dev lo root
sudo tc qdisc add dev lo root netem delay "${1}ms"
