# Running a local Casper network with NCTL

[NCTL](https://github.com/casper-network/casper-node/tree/release-1.4.3/utils/nctl) is a CLI application to control one or multiple Casper networks locally. Many developers wish to spin up relatively small test networks to localize their testing before deploying to the blockchain.

To simplify even more the set up of a local network, you may run NCTL within a docker container. To start a container and publish the ports of one the nodes, write the following command:

```bash
docker run --rm -it --name mynctl -d -p 11101:11101 -p 14101:14101 -p 18101:18101 makesoftware/casper-nctl
```

Refer to the [`casper-nctl-docker`](https://github.com/make-software/casper-nctl-docker/) repository for further details on how to use NCTL with docker.
