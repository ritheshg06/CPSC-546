$("#rowSlider").slider();
$("#columnSlider").slider();

var RowsCount = $("#RowsCount");
var ColumnsCount = $("#ColumnsCount");
$(RowsCount).html("Rows - " + $("#rowSlider").slider('getValue'));
$(ColumnsCount).html("Columns - " + $("#columnSlider").slider('getValue'));

createcontextMenu();
setHighlight();
$("#rowSlider").slider().on('change', function () {
    var rowVal = $("#rowSlider").slider('getValue');
    var colVal = $("#columnSlider").slider('getValue');
    $("#rows").val(rowVal);
    $("#columns").val(colVal);
    $(RowsCount).html("Rows - " + rowVal);
    $("#TotalSeats").html("Total Spots - " + (rowVal * colVal));

    var tableRowCount = $("#SeatsTable tbody tr").length;
    if (tableRowCount < rowVal) {
        for (var i = tableRowCount; i < rowVal; i++) {
            $("#SeatsTable tbody").append($("#SeatsTable tbody tr:last").clone());
            var r = $("#SeatsTable tbody").find("tr:last");
            var tds = r.find("td");
            $.each(tds, function (index, e) {
                $(e).attr("data-row", i);
                //$(e).html("(" + ($(e).data("column") + 1) + "," + (i + 1) + ")");
                $(e).html("-");
            });
            //r.find("td").html("_");
            r.find("td").removeAttr("style");
            r.find("td").removeClass("highlighted");
        }
    } else {
        for (var i = tableRowCount; i > rowVal; i--) {
            $("#SeatsTable tbody tr:last").remove();
        }
    }
    createcontextMenu();
    setHighlight();

});
$("#columnSlider").slider().on('change', function () {
    var rowVal = $("#rowSlider").slider('getValue');
    var colVal = $("#columnSlider").slider('getValue');
    $("#rows").val(rowVal);
    $("#columns").val(colVal);
    $(ColumnsCount).html("Columns - " + colVal);
    $("#TotalSeats").html("Total Spots - " + (rowVal * colVal));


    var tableColumnCount = $("#SeatsTable tbody tr:last td").length;
    if (tableColumnCount < colVal) {
        for (var i = tableColumnCount; i < colVal; i++) {
            var trs = $('#SeatsTable tbody tr');
            $.each(trs, function (index, e) {
                //$(e).append($("<td  data-column=\"" + i + "\" data-row=\"" + $(e).index() + "\">(" + (i + 1) + "," + ($(e).index() + 1) + ")</td>"));
                $(e).append($("<td  data-column=\"" + i + "\" data-row=\"" + $(e).index() + "\">-</td>"));
            });
            //$('#SeatsTable tbody tr').append($("<td data-column=\""+i+"\" data-row=\""+$('#SeatsTable tbody tr').index()+"\">_</td>"));
        }
    } else {
        for (var i = tableColumnCount; i > colVal; i--) {
            var r = $("#SeatsTable tbody").find("tr");
            r.find("td:last").remove();
        }
    }
    createcontextMenu();
    setHighlight();
});

function createcontextMenu() {
    $("#SeatsTable tbody td").contextmenu({
        target: "#contextMenu",
        onItem: function (context, e) {
            var option = $(e.target).data("action");
            switch (option) {
                case 'createCycleSeat':
                    {
                        var tds = $("#SeatsTable tbody td.highlighted");
                        var exceed = false;
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            if ($('cycle').length < 100) {
                                $(element).html("<cycle><div><input id=" + column + "_" + row + " type=\"number\" min=1 max=2500  /><br/><div></div></div></cycle>")
                                $(element).toggleClass("highlighted");
                                $('#ConfiguredSpots').html("Total Configured Seats - " + $('cycle').length);
                                $('#ConfiguredSeats').val($('cycle').length);
                            }
                            else {
                                exceed = true;
                            }

                        });
                        if (exceed) alert("Maximum 100 spots can be configured");

                        checkUnique();
                        $('input[type=number]').first().trigger("input");
                        break;
                    }
                case 'SetToMaintenanceSeat':
                    {
                        $("#SeatsTable tbody td.highlighted cycle").addClass("undermaintenance");
                        $("#SeatsTable tbody td.highlighted").toggleClass("highlighted");
                        break;
                    }
                case 'RecoverFromMaintenance':
                    {
                        $("#SeatsTable tbody td.highlighted cycle").removeAttr("class");
                        $("#SeatsTable tbody td.highlighted").toggleClass("highlighted");
                        break;
                    }
                case 'clearSeat':
                    {
                        // var c=$(invokedOn).parents("td").data("column");
                        // var r=$(invokedOn).parents("td").data("row");
                        // $(invokedOn).parents("td").html("(" + c + "," + r + ")");
                        var tds = $("#SeatsTable tbody td.highlighted");
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            //$(element).html("(" + (column + 1) + "," + (row + 1) + ")");
                            $(element).html("-");
                            $(element).toggleClass("highlighted");
                            $('#ConfiguredSpots').html("Total Configured Seats - " + $('cycle').length);
                            $('#ConfiguredSeats').val($('cycle').length);
                        });
                        checkUnique();
                        $('input[type=number]').first().trigger("input");
                        break;
                    }
                case 'clearSelection':
                    {
                        $("#SeatsTable tbody td.highlighted").toggleClass("highlighted");
                        break;
                    }
                case 'invertSelection':
                    {
                        $("#SeatsTable tbody td").toggleClass("highlighted");
                        break;
                    }
                case 'selectAllSeats':
                    {
                        $("#SeatsTable tbody td").addClass("highlighted");
                        break;
                    }
                case 'createFanSpot':
                    {
                        var tds = $("#SeatsTable tbody td.highlighted");
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            $(element).html("<fan><i class=\"icon md-toys\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></fan>");
                            $(element).toggleClass("highlighted");
                        });
                        break;
                    }
                case 'createTVSpot':
                    {
                        var tds = $("#SeatsTable tbody td.highlighted");
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            $(element).html("<tv><i class=\"icon md-tv\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></tv>");
                            $(element).toggleClass("highlighted");
                        });
                        break;
                    }
                case 'createSpeakerSpot':
                    {
                        var tds = $("#SeatsTable tbody td.highlighted");
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            $(element).html("<speaker><i class=\"icon md-speaker\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></speaker>");
                            $(element).toggleClass("highlighted");
                        });
                        break;
                    }
                case 'createInstructorSpot':
                    {
                        var tds = $("#SeatsTable tbody td.highlighted");
                        $.each(tds, function (index, element) {
                            var row = $(element).data("row");
                            var column = $(element).data("column");
                            $(element).html("<instructor><i class=\"fa fa-user \" style=\"font-size:36px;\" aria-hidden=\"true\"></i></instructor>");
                            $(element).toggleClass("highlighted");
                        });
                        break;
                    }
            }
        }
    });
}

function setHighlight() {
    var isMouseDown = false,
        isHighlighted;
    var tds = $("#SeatsTable tbody td");
    $.each(tds, function (index, element) {
        $(element).unbind('mousedown');
        $(element).unbind('mouseover');
        $(element).unbind('touchstart');
        $(element).unbind('touchmove');
        $(element).mousedown(function (event) {
            if (event.which == 1) {
                if (event.target.nodeName == "TD" || event.target.nodeName == "DIV" || event.target.nodeName == "I" || event.target.nodeName == "SPEAKER" || event.target.nodeName == "TV" || event.target.nodeName == "INSTRUCTOR") {
                    isMouseDown = true;
                    $(element).toggleClass("highlighted");
                    isHighlighted = $(element).hasClass("highlighted");
                }
            }
        });
        $(element).mouseover(function () {
            if (isMouseDown) {
                $(element).toggleClass("highlighted", isHighlighted);
            }
        });
    });


    $(document)
        .mouseup(function () {
            isMouseDown = false;
        });
}

function checkUnique() {
    var $map = [];

    $.when($('#SeatsTable').unbind('input'))
        .done(function () {
            $duplicates = [];
            $('input[type=number]').on('input', function () {

                $('input[type=number]').each(function () {

                    if ($(this).val().length == 0) {
                        $(this).addClass("hasError");
                        ERROR.SetUniqueError(true);
                    }
                    else {
                        $(this).removeClass("hasError");
                        ERROR.SetUniqueError(false);
                    }

                    $current = $(this);
                    $duplicates = $('#SeatsTable input[type=number]').filter(function (index) {
                        if ($(this).val() != "" && $current.val() != "") {
                            return $(this).val() == $current.val();
                        }
                    });

                    // if($current.val()!="")
                    // {
                    //     var val=$current.val();
                    //     $duplicates=$(":input[value=1]").addClass("hasError");
                    // }
                    if ($duplicates.length > 1) {
                        ERROR.SetUniqueError(true);
                        // $.each($duplicates,function(i,e){
                        //     $(e).addClass("hasError");
                        // });

                        $duplicates.addClass("hasError");
                    }

                });

                if ($('input[type=number].hasError').length > 1) {
                    ERROR.SetUniqueError(true);
                }
            });

        });

}
var ERROR = (function () {
    var Instance;
    function CreateInstance() {
        var object = { ErrorFlag: false };
        return object;
    }
    return {
        SetUniqueError: function (flag) {
            if (!Instance)
                Instance = CreateInstance();

            Instance.ErrorFlag = flag;
        },
        hasUniqueError: function () {
            if (!Instance)
                Instance = CreateInstance();
            return Instance.ErrorFlag;
        }
    };
})();