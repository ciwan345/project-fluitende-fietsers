function tijdweergave() {

    const zin = document.getElementById("opendicht");
    const dot = document.querySelector(".status-dot");
    const tijdstatus = document.getElementById("tijdstatus");

    // STOP als deze pagina geen winkelstatus heeft
    if (!zin || !dot || !tijdstatus) return;

    const nu = new Date();
    const tijd = nu.toLocaleTimeString("nl-NL", {
        hour: "2-digit",
        minute: "2-digit"
    });

    const uren = nu.getHours();
    const dag = nu.getDay(); // 0 = zondag, 6 = zaterdag

    if (dag === 0) {
        zin.textContent = "winkel gesloten";
        zin.style.color = "crimson";
        dot.style.backgroundColor = "red";
    }

    else if (dag === 6 && uren >= 9 && uren <= 17) {
        zin.textContent = "winkel geopend";
        zin.style.color = "green";
        dot.style.backgroundColor = "green";
    }

    else if (dag !== 6 && uren >= 9 && uren < 18) {
        zin.textContent = "winkel geopend";
        zin.style.color = "green";
        dot.style.backgroundColor = "green";
    }

    else {
        zin.textContent = "winkel gesloten";
        zin.style.color = "crimson";
        dot.style.backgroundColor = "red";
    }

    tijdstatus.textContent = tijd + " — ";
}
setInterval(tijdweergave, 1000);
tijdweergave();



window.onload = fietsbewegen()
function fietsbewegen()
{
    const bewegen = document.querySelector(".fietser")

    
    let x = 0;
    let richting = -1; // eerst naar links
    let rotate = 0

    setInterval(() => {
        x += richting * 3;
        // rotate += richting * -10
        let scale = 1
        

        
        if (x < -65) {
            richting = 1;
            scale = 1.3
                
        }
        else if (x < -50){
            scale = 1.2
        }
        else if (x < -35){
            scale = 1.2
        }
        else if (x < -25){
            scale = 1.1
        }


        
        if (x > 20) {
            richting = -1;
            scale = 0 

            
        }

        bewegen.style.transform = `translateX(${x}px) scale(${scale})`;
        
    }, 900);

    

    
}

const carts = document.querySelectorAll(".cart");
carts.forEach(cart => {
    cart.style.transition = "transform 0.4s ease-out";

    cart.addEventListener("click", () => {
        window.location.href = "winkelwagen.html";
    });
});




document.addEventListener("DOMContentLoaded", () => {

    const productCards = document.querySelectorAll(".product-card");
    const totaalElement = document.querySelector(".betaal-overzicht h3");

    function updateTotaal() {
        let totaal = 0;

        productCards.forEach(card => {
            const prijsText = card.querySelector("p").textContent; 
            const prijs = Number(prijsText.replace(/[^0-9]/g, "")); 
            const aantal = Number(card.querySelector("input").value);

            totaal += prijs * aantal;
        });

        totaalElement.textContent = `Totaal: € ${totaal}`;
    }

    productCards.forEach(card => {
        const input = card.querySelector("input");
        input.addEventListener("input", updateTotaal);
    });

    updateTotaal();
});


const inputvelden = document.querySelectorAll(".aantal");
const em = document.querySelectorAll("#veld");

inputvelden.forEach((input, md) => {
    input.addEventListener("input", ()=>{
        em[md].textContent = input.value;
    });
});


const subtotal = document.querySelectorAll("#subtotaal")

subtotal.forEach("")



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
