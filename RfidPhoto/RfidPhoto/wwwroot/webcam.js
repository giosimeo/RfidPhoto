function startVideo(src, facing) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia(
            { video: { facingMode: "user" } })
            .then(function (stream) {
                let video = document.getElementById(src);
                console.log(facing);
                if ("srcObject" in video) {
                    video.srcObject = stream;
                } else {
                    video.src = window.URL.createObjectURL(stream);
                }
                video.onloadedmetadata = function (e) {
                    video.play();
                };
                //mirror image
                video.style.webkitTransform = "scaleX(-1)";
                video.style.transform = "scaleX(-1)";
            });
    }
}

function getFrame(src, dest, dotNetHelper) {
    console.log("getFrame called");
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 390, 292);

    let dataUrl = canvas.toDataURL("image/jpeg");
    dotNetHelper.invokeMethodAsync('ProcessImage', dataUrl);
}

function getFrame2(src, dest, dotNetHelper) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 390, 292);

    let dataUrl = canvas.toDataURL("image/jpeg");
    dotNetHelper.invokeMethodAsync('ProcessImage2', dataUrl);
}

//navigator.mediaDevices.getUserMedia({
//    video: { facingMode: "user" }, // "user" = frontale, "environment" = posteriore
//    audio: false
//})
//    .then(stream => {
//        const videoElement = document.getElementById('video');
//        videoElement.srcObject = stream;
//    })
//    .catch(error => {
//        console.error("Errore nell'accesso alla webcam:", error);
//    });
