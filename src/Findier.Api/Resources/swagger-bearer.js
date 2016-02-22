$('#explore').off();

$('#explore').click(function () {
    var key = $('#input_apiKey')[0].value;
    var credentials = key.split(':'); //username:password expected

    $.ajax({
        url: "/api/oauth/token",
        type: "post",
        contenttype: 'x-www-form-urlencoded',
        data: "grant_type=password&username=" + credentials[0] + "&password=" + credentials[1],
        success: function (response) {
            var bearerToken = 'Bearer ' + response.access_token;

            window.swaggerUi.api.clientAuthorizations.add('Authorization', new SwaggerClient.ApiKeyAuthorization('Authorization', bearerToken, 'header'));
            window.swaggerUi.api.clientAuthorizations.remove("api_key");
            alert("Login successfull");
        },
        error: function (xhr, ajaxoptions, thrownerror) {
            alert("Login failed!");
        }
    });
});