DOCKER_ENV=''
DOCKER_TAG=''

case "$TRAVIS_BRANCH" in
  "master")
    DOCKER_ENV=production
    DOCKER_TAG=latest
    ;;
  "develop")
    DOCKER_ENV=development
    DOCKER_TAG=dev
    ;;
esac

# log into docker hub.
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD

# build the docker image.
docker build -f ./Bejebeje.Identity/Dockerfile.$DOCKER_ENV -t bejebeje/identity:$DOCKER_TAG ./Bejebeje.Identity --no-cache

# tag the docker image.
docker tag bejebeje/identity:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/identity:$DOCKER_TAG

# push the docker image to docker hub.
docker push $DOCKER_USERNAME/bejebeje/identity:$DOCKER_TAG