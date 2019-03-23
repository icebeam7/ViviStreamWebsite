var maxLength = 200;
var initialLength = $('#Message').val().length;
$('#MessageCount').text(initialLength);

$('#Message').keyup(function () {
    var length = $(this).val().length;
    //var left = maxLength - length;
    $('#MessageCount').text(length);
});