var slideIndex = 1;
var slides, timer;
function showSlides() {
    var i;
    slides = document.getElementsByClassName("mySlides");
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    slideIndex++;
    if (slideIndex > slides.length) { slideIndex = 1 }
    slides[slideIndex - 1].style.display = "block";
    slides[slideIndex - 1].style.animationName = "fade";
    slides[slideIndex - 1].style.animationDuration = "1.5s";
    //put the timeout in the timer variable
    timer = setTimeout(showSlides, 3000);
}

function plusSlides(position) {
    //clear/stop the timer
    clearTimeout(timer);
    slides = document.getElementsByClassName("mySlides");

    slideIndex += position;
    if (slideIndex > slides.length) { slideIndex = 1 }
    else if (slideIndex < 1) { slideIndex = slides.length }
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    slides[slideIndex - 1].style.display = "block";
    slides[slideIndex - 1].style.animationName = "fade";
    slides[slideIndex - 1].style.animationDuration = "1.5s";
    //create a new timer
    timer = setTimeout(showSlides, 3000);
}

function currentSlide(index) {
    //clear/stop the timer
    clearTimeout(timer);
    slides = document.getElementsByClassName("mySlides");

    if (index > slides.length) { index = 1 }
    else if (index < 1) { index = slides.length }
    //set the slideIndex with the index of the function
    slideIndex = index;
    for (i = 0; i < slides.length; i++) {
        slides[i].style.display = "none";
    }
    slides[index - 1].style.display = "block";
    slides[index - 1].style.animationName = "fade";
    slides[index - 1].style.animationDuration = "1.5s";
    //create a new timer
    timer = setTimeout(showSlides, 3000);
}

window.addEventListener('load', function () {
    showSlides();
});
