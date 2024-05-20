document.getElementById("addForm").addEventListener('submit', (e) => {
    e.preventDefault();

    const destinationName = document.getElementById("title")
    const imageInput = document.getElementById("image")
    const seasonName = document.getElementById("season")

    let errors = [];

    if (destinationName.value === "") {
        errors.push("Please enter a destination name.");
    }
    if (imageInput.value === "" || !/\.(jpg|jpeg|png|gif)$/.test(imageInput.value)) {
        errors.push("Please select a valid image file (jpg, jpeg, png, gif).");
    }

    if (errors.length > 0) {
        errors.forEach(error => {
            alert(error);
        });
        return;
    }

    var formData = new FormData();
    formData.append("destination", destinationName.value)
    formData.append("file", imageInput.files[0], imageInput.files[0].name)
    formData.append("season", seasonName.value)

    destinationName.value = ""
    seasonName.value = ""
    fetch("/destinations", {
        method: "POST",
        body: formData
    })
        .then(() => { })
        .catch(error => console.error("error:", error))
});

const removeForm = document.getElementById("removeForm");

removeForm.addEventListener('submit', function (event) {
    event.preventDefault();
    let selectedOption = document.getElementById('removePlaceSelect').value;

    console.log(selectedOption)
    fetch(`/destinations/${selectedOption}`, {
        method: 'DELETE',
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('HTTP error ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Success:', data);
        })
        .catch(error => {
            console.error('Error:', error);
        });
});