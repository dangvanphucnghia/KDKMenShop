$(function () {
    $('.search-input').on('keyup', function () {
        var searchText = $(this).val();

        // Check if there is no search keyword
        if (searchText.trim() === '') {
            $('.search-suggestions').empty(); // Clear the suggestion list
            return; // End the function when there's no keyword
        }

        $.ajax({
            url: '/Promotion/TimKiemSanPhamGiamGia',
            type: 'GET',
            dataType: 'json',
            data: { q: searchText },
            success: function (data) {
                // Process the returned data
                var suggestions = '';

                $.each(data, function (index, value) {
                    suggestions += '<li>' + value + '</li>';
                });

                $('.search-suggestions').html(suggestions);
            }
        });
    });

    // Handle when the user clears the search keyword
    $('.search-input').on('blur', function () {
        var searchText = $(this).val();

        if (searchText.trim() === '') {
            $('.search-suggestions').empty(); // Clear the suggestion list
        }
    });

    // Handle when the user clicks on a suggestion item
    $('.search-suggestions').on('click', 'li', function () {
        var suggestionText = $(this).text().trim(); // Get the content of the clicked item
        $('.search-input').val(suggestionText); // Set the content into the search input
        $('.search-suggestions').empty(); // Clear the suggestion list after selection
        $('.hero__search__form form').submit(); // Submit the form

    });
});
