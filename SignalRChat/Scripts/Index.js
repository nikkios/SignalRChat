var Chat;

$(function () {
    console.log("Starting up");

    InitChatHub();
    
    // Get the user name and store it to prepend to messages.
    $('#displayname').val(prompt('Enter your name:', ''));


    // Set initial focus to message input box.  
    $('#message').focus();
    
});

function InitChatHub() {

    // Declare a proxy to reference the hub. 
    Chat = $.connection.chatHub;

    // Create a function that the hub can call to broadcast messages.
    Chat.client.broadcastMessage = function (message) {
        WriteWall(message);
    };

    WriteWall = function(message) {
        // Html encode display name and message. 
        var encodedName = $('<div />').text(message.UserName).html();
        var encodedMsg = $('<div />').text(message.Text).html();
        // Add the message to the page. 
        $('#discussion').append('<li><strong>' + encodedName
            + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
    }

    Chat.client.setChatHistory = function (messages) {
        //console.log(messages);

        for (var i = 0; i < messages.length; i++) {
            console.log(messages[i]);
            WriteWall(messages[i]);

        }
    }

    Chat.client.updateUserList = function (users) {

        $('#users').empty();

        for (var i = 0; i < users.length; i++) {
            var encodedName = $('<div />').text(users[i].Title).html();
            $('#users').append('<li>' + encodedName + '</li>');
        }


    }


// Start the connection.
    $.connection.hub.start().done(function () {

        Chat.server.login($('#displayname').val());

        $('#sendmessage').click(function () {
            // Call the Send method on the hub. 
            Chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment. 
            $('#message').val('').focus();
        });
    });

};


