function loadCropper(id) {
    const image = document.getElementById(id);
    window.cropper = new Cropper(image, {
        zoomable: false,
        viewMode: 1,
        multiuple: true,
        crop(event) {
            DotNet.invokeMethodAsync('TwitchManager', 'ImageCropperSetDataAsync', event.detail.x, event.detail.y, event.detail.width, event.detail.height);
        },
    });
}


function getCroppedImage() {
    const canvas = window.cropper.getCroppedCanvas();
    const dataUrl = canvas.toDataURL();
    return dataUrl;
}