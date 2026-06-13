let headers = new Headers();
headers.append("Content-Type", "application/json");
headers.append("Accept", "application/json");
headers.append("Access-Control-Allow-Origin", "*");
headers.append("Access-Control-Allow-Credentials", "true");
headers.append("GET", "POST", "OPTIONS");

const API_BASE_URL = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1" || window.location.protocol === "file:"
    ? "http://localhost:5157/api"
    : "https://api.mh.mapa.tomekb530.me/api";

const header = document.querySelector(".header");
const hamburger = document.querySelector(".hamburger");
const bar1 = document.querySelector(".hamburger__bar-1");
const bar2 = document.querySelector(".hamburger__bar-2");
const bar3 = document.querySelector(".hamburger__bar-3");
const sidebar = document.querySelector(".sidebar");
const headerArrow = document.querySelector(".header__arrow");
const filterBtn = document.querySelector(".filter__btn");
const voivodeship = document.querySelector("#select-voivodeship");
const nameInput = document.querySelector(".header__search");

const addressInfoContainer = document.querySelector(".sidebar__address-info");
const detailedInfoContainer = document.querySelector(".sidebar__detailed-info");
const dropdownButton = document.querySelector(".dropdown-button");
const dropdownContent = document.querySelector(".dropdown-content");

const contentTab1 = document.querySelector("#content-1");
const contentTab2 = document.querySelector("#content-2");
const selectTab1 = document.querySelector("#tab1");
const selectTab2 = document.querySelector("#tab2");
const checkboxes = document.querySelectorAll("#school-type-checkbox");

let schools = [];
let markers = L.markerClusterGroup();

//map center point
let mapZoom = 5;

//set map zoom
window.innerWidth > 800 ? (mapZoom = 6) : (mapZoom = 5);
if (window.innerWidth > 800) {
    mapZoom = 6;
} else if (window.innerWidth > 1200) {
    mapZoom = 6;
}

const map = L.map("map").setView([51.9194, 19.1451], mapZoom);

//map definition and page connection
L.tileLayer(
    "https://api.maptiler.com/maps/streets/{z}/{x}/{y}.png?key=RakePZLBXp7DUCKOgD6V",
    {
        tileSize: 512,
        zoomOffset: -1,
        attribution:
            '\u003ca href="https://www.maptiler.com/copyright/" target="_blank"\u003e\u0026copy; MapTiler\u003c/a\u003e \u003ca href="https://www.openstreetmap.org/copyright" target="_blank"\u003e\u0026copy; OpenStreetMap contributors\u003c/a\u003e',
        crossOrigin: true,
    }
).addTo(map);

//fetch all schools while page loaded
document.onload = fetchAllSchools();

//EVENT LSITENERS

//filter on school type checkbox change
checkboxes.forEach((checkbox) =>
    checkbox.addEventListener("change", filterSchools)
);

//filter on voivodeship change
voivodeship.addEventListener("change", filterSchools);

//filter on input change with timeout to prevent lagging
nameInput.addEventListener("input", function () {
    nameInput.value.length < 2
        ? setTimeout(filterSchools, 700)
        : setTimeout(filterSchools, 100);
});

//show content on tabs click
selectTab1.addEventListener("click", () => {
    contentTab1.style.display = "block";
    contentTab2.style.display = "none";
});

selectTab2.addEventListener("click", () => {
    contentTab2.style.display = "block";
    contentTab1.style.display = "none";
});

//search dropdown
dropdownButton.addEventListener("click", () => {
    dropdownContent.classList.toggle("dropdown--invisible");
    dropdownContent.classList.toggle("dropdown--visible");
    if (!sidebar.classList.contains("sidebar--hide")) {
        sidebar.classList.toggle("sidebar--hide");
    }
});

//search bar arrow click
headerArrow.addEventListener("click", () => {
    bar1.classList.remove("change-bar1");
    bar2.classList.remove("change-bar2");
    bar3.classList.remove("change-bar3");
    if (dropdownContent.classList.contains("dropdown--invisible")) {
        headerArrow.classList.remove("arrow--up");
        headerArrow.classList.add("arrow--down");
    } else if (dropdownContent.classList.contains("dropdown--visible")) {
        headerArrow.classList.remove("arrow--down");
        headerArrow.classList.add("arrow--up");
    }
});

//sidebar hamburger
hamburger.addEventListener("click", () => {
    sidebar.classList.toggle("sidebar--hide");
    if (dropdownContent.classList.contains("dropdown--visible")) {
        dropdownContent.classList.toggle("dropdown--visible");
        dropdownContent.classList.toggle("dropdown--invisible");
    }
    if (dropdownContent.classList.contains("dropdown--invisible")) {
        headerArrow.classList.remove("arrow--up");
        headerArrow.classList.add("arrow--down");
        bar1.classList.toggle("change-bar1");
        bar2.classList.toggle("change-bar2");
        bar3.classList.toggle("change-bar3");
    }
});

//HANDLERS

//clear content tabs
function deleteChildNodes(container) {
    while (container.hasChildNodes()) {
        container.removeChild(container.lastChild);
    }
}

//get value from school type checkbox
function getCheckboxesValue(schoolTypes) {
    checkboxes.forEach((cb) => {
        if (cb.checked) {
            schoolTypes.push(cb.value.toUpperCase());
        }
    });
}

//handle create single marker
function createMarker(school) {
    const marker = L.marker([school.latitude, school.longtitude]).on(
        "click",
        handleMarkerClick
    );
    marker.bindPopup(createPopup(school.businessData));
    marker.myID = school.id;
    markers.addLayer(marker);
}

//load all schools handle
function fetchAllSchools() {
    fetch(`${API_BASE_URL}/Schools`, {
        method: "GET",
        headers: headers,
    })
        .then((res) => res.json())
        .then((data) => {
            //write all schools to arr
            schools = Array.isArray(data) ? data : (data && data.$values ? data.$values : []);
            //create marker for every school loaded
            schools.forEach((school) => {
                createMarker(school);
            });
            map.addLayer(markers);
        });
}

//filter schools
function filterSchools() {
    markers.clearLayers();
    const voivodeshipSelect = voivodeship.value.toUpperCase();
    const typedValue = nameInput.value.toUpperCase();
    let schoolTypes = [];
    const filters = ["nazwa", "miejscowosc", "poczta"];
    getCheckboxesValue(schoolTypes);

    schools.forEach((school) => {
        const data = school.businessData;
        if (
            (data.wojewodztwo.toUpperCase() === voivodeshipSelect ||
                voivodeshipSelect === "ALL-SCHOOLS") &&
            schoolTypes.some((type) => type === data.typ.toUpperCase()) &&
            (data[filters[0]].toUpperCase().includes(typedValue) ||
                data[filters[1]].toUpperCase().includes(typedValue) ||
                data[filters[2]].toUpperCase().includes(typedValue))
        ) {
            createMarker(school, data);
        }
    });
    map.addLayer(markers);
}

//click on marker handle
function handleMarkerClick(e) {
    //clear details content
    deleteChildNodes(addressInfoContainer);
    deleteChildNodes(detailedInfoContainer);
    //get id of target marker
    const targetID = e.sourceTarget.myID;
    //get school with target id from DB
    fetch(`${API_BASE_URL}/Schools/${targetID}`, {
        method: "GET",
        headers: headers,
    })
        .then((res) => res.json())
        .then((data) => {
            const schoolData = data.businessData;
            const schoolObj = translateDetailsNaming(schoolData);
            //show sidebar on desktop
            if (window.innerWidth >= 1366) {
                if (sidebar.classList.contains("sidebar--hide")) {
                    sidebar.classList.toggle("sidebar--hide");
                }
            }
            createTabs(schoolObj);
        });
}

//create popup handle
function createPopup(data) {
    const schoolPopup = `
      <div class="popup">

        <p class="popup__school-name">${data.nazwa}</p>

        <p class="popup__data popup__city">
        ${data.miejscowosc === data.poczta && data.ulica != null
            ? ""
            : data.miejscowosc
        } ${data.ulica !== null ? "" : data.numerBudynku}</p>

        <p class="popup__data popup__address">${data.ulica !== null ? data.ulica + " " + data.numerBudynku : ""
        } ${data.numerLokalu != null && data.numerLokalu !== "" ? " lok. " + data.numerLokalu : ""}</p>

        <p class="popup__data popup__city">${data.kodPocztowy} ${data.poczta
        }</p>
    
        <a class=" popup__datapopup__website" target="_blank" href="${data.stronaInternetowa
        }" >
        ${data.stronaInternetowa !== null ? data.stronaInternetowa : ""}</a>

        <a class=" popup__datapopup__website" href="mailto:${data.email}" >
        ${data.email !== null ? data.email : ""}</a>
      </div>
      `;

    return schoolPopup;
}

//create content tabs
function createTabs(schoolObj) {
    let category = "";
    let text = "";
    for (let i = 0; i < 2; i++) {
        i == 0 ? (category = "addressInfo") : (category = "detailedInfo");

        Object.keys(schoolObj[category]).map((key) => {
            const value =
                schoolObj[category][key] === null ? "" : schoolObj[category][key];

            const itemContainer = document.createElement("div");
            itemContainer.classList.add("sidebar__info-item");

            const itemKey = document.createElement("p");
            itemKey.classList.add("sidebar__item-key");
            itemKey.innerText = key + ": ";

            const itemValue = document.createElement("p");
            itemValue.classList.add("sidebar__item-value");

            //Rewrite to string if there is an array of object
            if (typeof value === "object") {
                text = "";
                for (let i = 0; i < value.length; i++) {
                    if (i < value.length - 1) {
                        text += value[i] + ", ";
                    } else {
                        text += value[i];
                    }
                }
            } else {
                text = value;
            }

            itemValue.innerText = text;
            itemContainer.appendChild(itemKey);
            itemContainer.appendChild(itemValue);

            i == 0
                ? addressInfoContainer.appendChild(itemContainer)
                : detailedInfoContainer.appendChild(itemContainer);
        });
    }
}

function translateDetailsNaming(schoolData) {
    return {
        addressInfo: {
            "Nazwa szkoĹy": schoolData["nazwa"],
            MiejscowoĹÄ: schoolData["miejscowosc"],
            "Kod pocztowy": schoolData["kodPocztowy"],
            Poczta: schoolData["poczta"],
            WojewĂłdztwo: schoolData["wojewodztwo"],
            Powiat: schoolData["powiat"],
            Gmina: schoolData["gmina"],
            Ulica: schoolData["ulica"],
            "Numer budynku": schoolData["numerBudynku"],
            "Numer lokalu": schoolData["numerLokalu"],
            Dyrektor: schoolData["dyrektor"],
            Telefon: schoolData["telefon"],
            Faks: schoolData["faks"],
            "Adres e-mail": schoolData["email"],
            "Strona WWW": schoolData["stronaInternetowa"],
            "Typ szkoĹy": schoolData["typ"],
            Przeznaczenie: schoolData["kategoriaUczniow"],
            "Status publicznoĹci": schoolData["statusPublicznosc"],
            "Liczba uczniĂłw": schoolData["liczbaUczniow"],
            "Nauczane jÄzyki": schoolData["jezykiNauczane"],
            "Tereny sportowe": schoolData["terenySportowe"],
        },
        detailedInfo: {
            "Struktura miejsca": schoolData["strukturaMiejsce"],
            "Rodzaj miejscowoĹci": schoolData["rodzajMiejscowosci"],
            "Specyfika placĂłwki": schoolData["specyfikaPlacowki"],
            "Numer RSPO": schoolData["rspoNumer"],
            Regon: schoolData["regonPodmiotu"],
            "NIP podmiotu": schoolData["nipPodmiotu"],
            "Miejsce w strukturze": schoolData["strukturaMiejsce"],
            "Data zaĹoĹźenia": schoolData["dataRozpoczeciaDzialalnosci"],
            "Data likwidacji": schoolData["dataLikwidacji"],
            "Kod terytorialny (MiejscowoĹÄ)":
                schoolData["kodTerytorialnyMiejscowosc"],
            "Kod terytorialny (Gmina)": schoolData["kodTerytorialnyGmina"],
            "Kod terytorialny (Powiat)": schoolData["kodTerytorialnyPowiat"],
            "Kod terytorialny (WojewĂłdztwo)":
                schoolData["kodTerytorialnyWojewodztwo"],
            "Podmiot nadrzÄdny (Nazwa)": schoolData["podmiotNadrzednyNazwa"],
            "Podmiot nadrzÄdny (Typ)": schoolData["podmiotNadrzednyTyp"],
            "Podmiot nadrzÄdny (RSPO)": schoolData["podmiotNadrzednyRspo"],
            "Organ prowadzÄcy (Nazwa)": schoolData["organProwadzacyNazwa"],
            "Organ prowadzÄcy (NIP)": schoolData["organProwadzacyNip"],
            "Organ prowadzÄcy (Regon)": schoolData["organProwadzacyRegon"],
            "Organ prowadzÄcy (Typ)": schoolData["organProwadzacyTyp"],
            "Organ prowadzÄcy (Gmina)": schoolData["organProwadzacyGmina"],
            "Organ prowadzÄcy (Powiat)": schoolData["organProwadzacyPowiat"],
            "Organ prowadzÄcy (WojewĂłdztwo)":
                schoolData["organProwadzacyWojewodztwo"],
        },
    };
}