echo "Current working directory is:"
pwd
cd /var/www/html/identity.bejebeje.com/
echo "Now the current working directory is:"
pwd
echo "running docker-compose down"
docker-compose down
echo "cleaning the volume"
docker volume rm identitybejebejecom_data-volume
echo "running docker-compose up"
docker-compose pull && docker-compose up -d