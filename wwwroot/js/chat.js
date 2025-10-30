async function processUserInput() {
    const userInput = $('#userInput').val().trim();
    if (!userInput) return;
    try {
        const response = await fetch('/Chat/Ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ question: userInput })
        });

        if (!response.ok) throw new Error('Network response was not ok.');
        console.log(response);
        const data = await response.json();
        console.log('data', data);
        $('#chatWindow').append(`<div class="user-message">${userInput}</div>`);
        $('#chatWindow').append(`<div class="bot-message">
        ${data.answer} <br />
        Confidence: ${(data.confidence * 100).toFixed(2)}%
    </div>`);

    } catch (error) {
        $('#chatWindow').append(`<div class="bot-message error">Error: ${error.message}</div>`);
    }
    $('#chatWindow').scrollTop($('#chatWindow')[0].scrollHeight);
    $('#userInput').val('');
}

function processCSVUpload() {
    var fileInput = $('#fileUpload')[0];
    if (fileInput.files.length === 0) {
        $('#uploadStatus').text('Please select a file first.');
        return;
    }

    var formData = new FormData();
    formData.append('csvFile', fileInput.files[0]);

    $.ajax({
        url: '/Chat/UploadCsv', // adjust for your actual controller
        type: 'POST',
        data: formData,
        processData: false, // important for FormData
        contentType: false,
        beforeSend: function () {
            $('#uploadStatus').text('Uploading and retraining...');
        },
        success: function (response) {
            if (response.success) {
                $('#uploadStatus').text(response.message);
            } else {
                $('#uploadStatus').text('Error: ' + response.message);
            }
        },
        error: function (xhr) {
            $('#uploadStatus').text('Unexpected error: ' + xhr.responseText);
        }
    });
}