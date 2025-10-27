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

        const data = await response.json();
        $('#chatWindow').append(`<div class="user-message">${userInput}</div>`);
        $('#chatWindow').append(`<div class="bot-message">${data.answer}</div>`);

    } catch (error) {
        $('#chatWindow').append(`<div class="bot-message error">Error: ${error.message}</div>`);
    }
    $('#chatWindow').scrollTop($('#chatWindow')[0].scrollHeight);
    $('#userInput').val('');
}