$(document).ready(function () {
    // Initialize select2 on the "Request Type" dropdown
    $("#selTransactionStatus").select2({
        placeholder: "-- Select a Transaction Status --", // Placeholder text
        allowClear: true, // Allow clearing the selection
    });

    // Set the initial selection to the placeholder
    $("#selTransactionStatus").val(null).trigger('change');

    // Handle the change event for the "Request Type" dropdown
    $("#selTransactionStatus").change(function () {
        var selectedValue = $(this).val();
        if (selectedValue) {
        }
    });
});