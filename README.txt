*) Palaist API ar Docker
Palaižam Docker Dektop uz lokāla datora
VisualStudio projekts "docker-compose" ir jāpalaiž Debug režīmā
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build

*) Pārbaudam vai API strādā
http://localhost:33118/_api/parameters

*) Angualr App:
Node verija
	Node v14.15.0

Angular aplikācijas palaišana:
cd .\Izm.Rumis\Izm.Rumis.App\
cd .\ClientApp\

Izpildam pirmo reizi:
npm install

Palaižam Angular:
npm start

*) Autentificēšanās
username: admin
password: dev
Vai poga ADFS