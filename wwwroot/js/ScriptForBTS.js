var slideIndexBST = 0;
var slidesBST, timer;

function showSlidesBST() {
    slidesBST = document.getElementsByClassName("mySlidesBST");
    for (var i = 0; i < slidesBST.length; i++) {
        slidesBST[i].style.display = "none";
    }
    slideIndexBST++;
    if (slideIndexBST > slidesBST.length) { slideIndexBST = 1; }
    slidesBST[slideIndexBST - 1].style.display = "block";
    slidesBST[slideIndexBST - 1].style.animationName = "fade";
    slidesBST[slideIndexBST - 1].style.animationDuration = "1.5s";
    timer = setTimeout(showSlidesBST, 3000);
}

function plusSlidesBST(n) {
    clearTimeout(timer);
    slideIndexBST += n;
    if (slideIndexBST > slidesBST.length) { slideIndexBST = 1; }
    else if (slideIndexBST < 1) { slideIndexBST = slidesBST.length; }
    for (var i = 0; i < slidesBST.length; i++) {
        slidesBST[i].style.display = "none";
    }
    slidesBST[slideIndexBST - 1].style.display = "block";
    slidesBST[slideIndexBST - 1].style.animationName = "fade";
    slidesBST[slideIndexBST - 1].style.animationDuration = "1.5s";
    timer = setTimeout(showSlidesBST, 3000);
}

function currentSlideBST(n) {
    clearTimeout(timer);
    if (n > slidesBST.length) { n = 1; }
    else if (n < 1) { n = slidesBST.length; }
    //set the slideIndex with the index of the function
    slideIndexBST = n;
    for (var i = 0; i < slidesBST.length; i++) {
        slidesBST[i].style.display = "none";
    }
    slidesBST[n - 1].style.display = "block";
    slidesBST[slideIndexBST - 1].style.animationName = "fade";
    slidesBST[slideIndexBST - 1].style.animationDuration = "1.5s";
    timer = setTimeout(showSlidesBST, 3000);
}

window.addEventListener('load', function () {
    showSlidesBST();
});
