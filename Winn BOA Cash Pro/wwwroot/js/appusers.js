/* eslint-disable @typescript-eslint/no-this-alias */
$(function () {
    $.fn.dataTable.moment();
    // Setup - add a text input to each footer cell with the text-input css class
    $('#tblUsers tfoot th.text-input').each(function () {
        var title = $(this).text().trim();
        $(this).html('<input type="text" placeholder="Search ' + title + '" />');
    });

    var table = $('#tblUsers').DataTable({
        dom: 'Qlfrtip',
        order: [[2, "desc"]],
        paging: true,
        searching: true,
        columnDefs: [
            {
                targets: '_all',
                className: 'dt-body-nowrap',
                render: $.fn.dataTable.render.ellipsis(20)
            },
            {
                targets: -1,
                data: null,
                defaultContent: "<button class='btn btn-primary'>Manage roles</button>"
            },
            {
                targets: 0,
                visible: false
            }
        ],
        initComplete: function () {
            // Apply the search
            this.api().columns().every(function () {
                var that = this;

                $('input', this.footer()).on('keyup change clear', function () {
                    if (that.search() !== this.value) {
                        that
                            .search(this.value)
                            .draw();
                    }
                });
            });
        }
    });
    $('#tblUsers tbody').on('click', 'button', function () {
        var data = table.row($(this).parents('tr')).data();
        window.location.href = "/AppUsers/Manage?userId=" + data[0];
    });
    //#region Search
    //set dropdown list
    table.column(4).every(function () {
        var column = this;
        var select = $('<select><option value="">Select</option></select>')
            .appendTo($(column.footer()).empty())
            .on('change', function () {
                var val = $.fn.dataTable.util.escapeRegex(
                    $(this).val()
                );
                column
                    .search(val ? '^' + val + '$' : '', true, false)
                    .draw();
            });
        column.data().unique().sort().each(function (d, j) {
            select.append('<option value="' + d + '">' + d + '</option>')
        });
    });

    //add search options under headers
    $('#tblUsers tfoot tr').appendTo('#tblUsers thead');
    //#endregion

    //#region employee dropdown
    //initialize select2 on Employee Name dropdown list
    $("#selEmployee").select2({
        placeholder: "-- Select Employee --",
        allowClear: true,
        templateResult: templateEmployeePickList,
        templateSelection: templateEmployeeSelectedList
    });
    $("#selEmployee").change(function () {
    var selectionText = $(this).find(":selected").text().split("|");
        $("#inpAdminEmail").val(selectionText[2]);
    });
    //#endregion

    $('#tblUsers thead').addClass('bg-primary');
});

function templateEmployeePickList(item) {
    //if (!item.empName) { return item.text; }
    var selectionText = item.text.split("|");
    var $returnString = $('<span><b>' + selectionText[0] + '</b><br /><em>' + selectionText[1] + '</em></span>');
    return $returnString;
};

function templateEmployeeSelectedList(item) {
    //if (!item.empName) { return item.text; }
    var selectionText = item.text.split("|");
    var $returnString = $('<span>' + selectionText[0] + '</span>');
    return $returnString;
};