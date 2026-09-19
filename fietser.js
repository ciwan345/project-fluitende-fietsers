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


function fietsbewegen()
{
    const bewegen = document.querySelector(".fietser")

    bewegen.style.backgroundColor = 'blue'

    
}