$(document).ready(function(){
    $('.dropdown-toggle').click(function(){
        var target = $(this).data('target');
        $(target).collapse('toggle');
    });
});
