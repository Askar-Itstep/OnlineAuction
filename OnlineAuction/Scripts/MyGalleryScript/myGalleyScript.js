var current = document.querySelector("#current");
console.log(current)

var listImg = document.querySelectorAll(".list-img img");
//console.log(listImg)
var opacity = 0.6;

listImg[0].style.opacity = opacity;

var thumbPointer = listImg[0];

listImg.forEach(img => img.addEventListener("click", imgClick));

function imgClick(e) {
    //console.log(e.target)
    current.src = e.target.src;
    current.classList.remove("fade-in");
    current.classList.add("fade-in");
    e.target.style.opacity = opacity;
    thumbPointer.style.opacity = 1;
    thumbPointer = e.target;
}
