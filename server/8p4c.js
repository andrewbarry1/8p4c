var ws;
var funcs = ["Shake the camera!", "Increase attack strength!",
	     "Zoom out!", "Lower the platform!", "Invert Controls!",
	    "Rotate the camera!","Create force!","Create explosions!"];
var letters = ["1","2","3","4","5","6","7","8"];
var f = 0;
var tapping = false;

window.onload = function() {
    ws = new WebSocket("ws://8p4c.andrewbarry.me/ws");
    ws.onopen = function() {
	$('#info').text("Connected, waiting for game.");
	ws.send("p");
    }
    ws.onmessage = onMessage;
    $('#body').on('tap', function(e) {
	e.preventDefault();
    });
    $.mobile.loading().hide();
}


function onMessage(m) {
    m = m.data;
    console.log(m);
    if (m == "c") {
	$('#info').text("Wait");
	$('#body').css("background-color","red");
    }
    else if (m == "q") {
	$('#info').text("Connected, waiting for game.");
	$('#body').css("background-color","white");
    }

    else if (m == "g") {
	tapping = true;
	$('#body').css("background-color","green");
	f = Math.floor((Math.random() * 8));
	$('#info').text(funcs[f]);
	$('#body').on('tap', function() {
	    console.log(letters[f]);
	    if (tapping) {
		ws.send(letters[f]);
		e.preventDefault();
	    }
	});
    }
    else if (m == "s") {
	$('#body').off('tap');
	$('#body').css("background-color","red");
	$('#info').text("Wait");
	tapping = false;
    }


}
