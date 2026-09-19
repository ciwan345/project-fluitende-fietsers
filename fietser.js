function tijdweergave() {

    const nu = new Date();
    const tijd = nu.toLocaleTimeString("nl-NL", {
        hour: "2-digit",
        minute: "2-digit"
    });

    const uren = nu.getHours();
    const zin = document.getElementById("opendicht");
    const dot = document.querySelector(".status-dot");


    if (uren >= 10 && uren < 18) {
        zin.textContent = "winkel geopend";
        zin.style.color = "green";
        dot.style.backgroundColor = "green"; 
    } else {
        zin.textContent = "winkel gesloten";
        zin.style.color = "crimson";
        dot.style.backgroundColor = "red"; 

      
    }

    document.getElementById("tijdstatus").textContent = tijd + " — ";
}

setInterval(tijdweergave, 1000);
tijdweergave();



//..................Foto carousel maken op de hoofdpagina ................................



const images = document.querySelectorAll(".carousel-image");
const dots = document.querySelectorAll(".dot");

const nextButton = document.querySelector(".next");
const prevButton = document.querySelector(".prev");

let currentImage = 0;


function showImage(index) {

    images.forEach(function(image) {
        image.classList.remove("active");
    });

    dots.forEach(function(dot) {
        dot.classList.remove("active");
    });

    images[index].classList.add("active");
    dots[index].classList.add("active");

    currentImage = index;
}


/* Volgende foto */

nextButton.addEventListener("click", function() {

    currentImage++;

    if (currentImage >= images.length) {
        currentImage = 0;
    }

    showImage(currentImage);

});


/* Vorige foto */

prevButton.addEventListener("click", function() {

    currentImage--;

    if (currentImage < 0) {
        currentImage = images.length - 1;
    }

    showImage(currentImage);

});


/* Automatisch wisselen */

setInterval(function() {

    currentImage++;

    if (currentImage >= images.length) {
        currentImage = 0;
    }

    showImage(currentImage);

}, 4000);