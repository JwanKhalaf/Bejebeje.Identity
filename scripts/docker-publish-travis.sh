DOCKER_TAG='latest'

# log into docker hub.
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD

# build the docker image.
docker build -f ./Bejebeje.Identity/Dockerfile -t bejebeje/identity:$DOCKER_TAG -t bejebeje/identity:$TRAVIS_BUILD_NUMBER ./Bejebeje.Identity --no-cache

# tag the docker image with latest.
docker tag bejebeje/identity:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/identity:$DOCKER_TAG

# tag the docker image with build number.
docker tag bejebeje/identity:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/identity:$TRAVIS_BUILD_NUMBER

# push the docker image (tagged latest) to docker hub.
docker push bejebeje/identity:$DOCKER_TAG

# push the docker image (tagged with build number) to docker hub.
docker push bejebeje/identity:$TRAVIS_BUILD_NUMBER