function RenderClassDetailsWithLayout(classID, roomID) {
    $("#initMsg").hide();
    $("#info").html('');
    $('#spots').html('');
    $("#actions").html('').hide().removeAttr("disabled", "disabled").removeClass("disabled").css({ "opacity": "1", "pointer-events": "auto" });;
    
    $.ajax({
        url: '/admin/getClassReservationData',
        method: 'GET',
        data: { classID: classID, roomID: roomID },
        success: function (data) {
            var classInfo = data.classDetails;
            var reservationDetails = data.reservationDetails;
            var roomLayout = data.roomLayout;

            $('div[data-classid="'+classID+'"][data-roomid="'+roomID+'"]').find('stat').text('('+reservationDetails.length+'/'+classInfo.classType.maxCapacity+')');

            $("#spots").append(
                '<div id="layout" style="margin-top:20px;"><table id="BookingSeatsTable" data-classid="' + classID + '" data-roomid="' + roomID + '" class="table" draggable="false" style="margin:0 auto;opacity: 1; pointer-events: auto;"><tbody></tbody></table></div>');

            var substituteInstructor = {
                firstName: "",
                lastName: ""
            }
            if (classInfo.instructorID != null && classInfo.instructorID != 0) {
                substituteInstructor.firstName = classInfo.substituteInstructor.firstName;
                substituteInstructor.lastName = classInfo.substituteInstructor.lastName;
            }

            var startTime = formatAMPM(classInfo.startDate.split('T')[0] + 'T' + classInfo.startTime + 'Z');
            var startDateTime = new Date(classInfo.startDate.split('T')[0] + 'T' + classInfo.startTime);
            var endDateTime = new Date(classInfo.endDate.split('T')[0] + 'T' + classInfo.endTime);
            var diffMs = (endDateTime - startDateTime);
            var diffMins = diffMs / 60000;
            var infoContent =
                '<h3 style="font-weight:bold;">' + classInfo.classType.className + '</h3>' +
                '<h3 id="DescriptionText" style="font-weight:bold;font-size:14px;">' + classInfo.classType.classDescription + '</h3>' +
                '<h4 style="font-weight:bold;">Instructor : ' + classInfo.classType.instructor.firstName + ' ' + classInfo.classType.instructor.lastName + '</h4>' +
                '<h4 style="font-weight:bold;">Substitute Instructor : ' + ((classInfo.instructorID == null || classInfo.instructorID == 0) ? "None" : (substituteInstructor.firstName + ' ' + substituteInstructor.lastName)) + '</h4>' +
                '<h4 style="font-weight:bold;">Date : ' + classInfo.startDate.split('T')[0].split('-')[1] + '-' + classInfo.startDate.split('T')[0].split('-')[2] + '-' + classInfo.startDate.split('T')[0].split('-')[0] + '</h4>' +
                '<h4 style="font-weight:bold;">Time : ' + startTime.toUpperCase() + ' | Duration : ' + diffMins + ' mins. | Location : ' + loc.locationName + '</h4>' +
                '<div class="row" style="padding-right: 13px; margin-bottom: -13px;">' +
                '<div id="CancelClassBtnDiv" class="col-lg-2 col-md-2 col-sm-12 col-xs-12">' +
                '<button data-classid="' + classInfo.classID + '" id="CancelClassBtn" class="btn btn-danger">Cancel Class</button>' +
                '</div>' +
                '<div id="SubstituteArea" class="col-lg-5 col-md-5 col-sm-12 col-xs-12">' +
                '<div class="col-lg-6 col-md-6 col-sm-12 col-xs-12" style="padding:0px;">' +
                '<select id="instructorsFilter" style="width:100%;">' +
                '</select>' +
                '</div>' +
                '<div class="col-lg-6 col-md-6 col-sm-12 col-xs-12" style="padding-left:10px;">' +
                '<button data-cid="' + classInfo.classID + '" id="SetSubstitute" class="btn btn-danger">Add Substitute</button>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '<hr style="width:100%;border:1px solid black;">';

            $("#info").append(infoContent);
            $("#info").show();

            var substituteInstructors = [];
            substituteInstructors.push({ id: 0, text: "None" });
            $.each(instructors, function (i, e) {
                substituteInstructors.push({ id: e.userID, text: e.firstName + ' ' + e.lastName });
            });
            $("#instructorsFilter").select2();
            $("#instructorsFilter").select2('destroy');
            $("#instructorsFilter").select2({
                placeholder: "Select Substitute Instructor",
                data: substituteInstructors
            });
            $("#instructorsFilter").val(0).trigger('change');

            var Rows = parseInt(roomLayout.rows);
            var Columns = parseInt(roomLayout.columns);
            for (i = 0; i < Rows; i++) {
                $("#BookingSeatsTable tbody").append("<tr></tr>");
                for (j = 0; j < Columns; j++) {
                    $("#BookingSeatsTable tbody tr:last").append("<td data-row=\"" + i + "\" data-column=\"" + j + "\" class=\"padded\"></td>");
                }
            }

            var SeatMatrix = JSON.parse(roomLayout.seatMatrix);
            $.each(SeatMatrix, function (index, element) {
                if (element != "-") {
                    var cell = $("#BookingSeatsTable tbody tr").find("td[data-column='" + element.C + "'][data-row='" + element.R + "']");
                    if (element.S == "fan") {
                        $(cell).html("<fan><i class=\"icon md-toys\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></fan>");
                    }
                    else if (element.S == "tv") {
                        $(cell).html("<tv><i class=\"icon md-tv\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></tv>");
                    }
                    else if (element.S == "speaker") {
                        $(cell).html("<speaker><i class=\"icon md-speaker\" aria-hidden=\"true\" style=\"font-size:36px;\"></i></speaker>");
                    }
                    else if (element.S == "instructor") {
                        $(cell).html("<instructor><i class=\"fa fa-user\" style=\"font-size:36px;\" aria-hidden=\"true\"></i></instructor>");
                    }
                    else if (element.S == "C") {
                        var rData = $(reservationDetails).filter(function (reserveindex) {
                            return element.SN == reservationDetails[reserveindex].seatNumber;
                        });
                        if (rData.length > 0) {
                            if (rData[0].reservationStatus == "3")
                                $(cell).html("<div id=\"" + element.C + "_" + element.R + "\" data-spot=\"" + element.SN + "\" class=\"undermaintenance\">" + element.SN + "</div>");
                            else {
                                var MemberName = rData[0].user.firstName + " " + rData[0].user.lastName;
                                var style = "";
                                //if(rData[0].EntryCode==1)
                                //{
                                //    style="color:darkred;";
                                //}


                                if (rData[0].signedIn == "1") {
                                    $(cell).html("<spot id=\"" + element.C + "_" + element.R + "\" style=\"" + style + "\" data-checkedin=\"true\" data-spot=\"" + element.SN + "\" class=\"reserved\"><checkin>&#10003;</checkin><number>" + element.SN + "</number><client data-resID=\"" + rData[0].reservationID + "\" data-userID=" + rData[0].userID + ">" + MemberName + "</client></spot>");
                                }
                                else {
                                    $(cell).html("<spot id=\"" + element.C + "_" + element.R + "\" style=\"" + style + "\" data-checkedin=\"false\" data-spot=\"" + element.SN + "\" class=\"reserved\"><number>" + element.SN + "</number><client data-resID=\"" + rData[0].reservationID + "\" data-userID=" + rData[0].userID + ">" + MemberName + "</client></spot>");
                                }

                            }
                        }
                        else {
                            $(cell).html("<a id=\"" + element.C + "_" + element.R + "\" data-spot=\"" + element.SN + "\" class=\"square\">" + element.SN + "</a>");
                        }
                    }
                }
            });

            $("#spots").show();
            $("#spots").trigger("SeatsRendered");
        }
    });
}

$("#spots").on('SeatsRendered', function () {

    $("body").attr("style", "cursor:default !important");
    $("#BookingSeatsTable").removeAttr("disabled").removeClass("disabled").css({ "opacity": "1", "pointer-events": "auto" });

    $("#BookingSeatsTable spot").click(function (e) {
        if ($(this).hasClass("QualifyForSwap"))
        {

        }
        else
        {
            $("#actions").hide();
            $("#BookingSeatsTable spot").removeClass("highlighted");
            $("#BookingSeatsTable a").removeClass("highlighted moveTo");
    
            var spotNumber = $(this).data("spot");
            $(this).addClass("highlighted");
            $("#actions").html('');
            if ($(this).hasClass("reserved")) {
                var client = $(this).find("client");
                var userID = $(client).data("userid");
                $("#actions").append('<h2>Seat is reserved for - ' + $(client).html() + '</h2>');
                $("#actions").append('<div id="OptionGrid" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;"></div>');

                if(userID!=0)
                    $("#OptionGrid").append('<button id="CancelReservation" class="btn btn-danger" style="width:auto;margin-right:10px;">Cancel Member\'s Reservation</button>');
                //$("#OptionGrid").append('<button id="MoveToSpot" class="btn btn-danger" style="width:auto;margin-right:10px;">Change Member\'s Seat</button>');
                //$("#OptionGrid").append('<button id="SwapMemberSpots" class="btn btn-danger" style="width:auto;margin-right:10px;">Swap Spots</button>');
                //$("#OptionGrid").append('<button id="CancelAction" class="btn btn-danger" style="width:auto;margin-right:10px;display:none;">Cancel</button>');

                if($(this).data("checkedin")==true || $(this).data("checkedin")=="true")
                {
                    //$("#OptionGrid").append('<button id="CheckOutBtn" class="checkInOutBtn btn btn-danger" style="width:auto;margin-right:10px;">Check - Out</button>');
                }
                else
                {
                    //$("#OptionGrid").append('<button id="CheckInBtn" class="checkInOutBtn btn btn-danger" style="width:auto;margin-right:10px;">Check - In</button>');
                }
                //$("#OptionGrid").append('<button id="ProfileViewBtn" class="btn btn-danger" style="width:auto;margin-right:10px;">View Profile</button>');
                $("#ActionGrid").append('<div id="StatusMessageGrid" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;"></div>');

                $("#CancelReservation").click(function () {
                    $("body").attr("style","cursor:progress !important");
                    $("#BookingSeatsTable").attr("disabled","disabled").addClass("disabled").css({"opacity":"0.5","pointer-events":"none"});
                    
                    var class_id = $("#BookingSeatsTable").data("classid");
                    var room_id = $("#BookingSeatsTable").data("roomid");
                    var res_id = $(client).data("resid");
      
                    $.ajax({
                        url: '/admin/bookingOperation',
                        type: "post",
                        data: { operationID: 2, locationID: loc.locationID, classID: class_id, roomID: room_id, reservationID: res_id, spotNumber: spotNumber, userID: userID },
                        success: function (data) {
                            if (data.statusCode == "Success") {
                                $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                                setTimeout(function () {
                                    RenderClassDetailsWithLayout(class_id, room_id);
                                }, 1500);
                            }
                            if (data.statusCode == "Error") {
                                $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                                setTimeout(function () {
                                    RenderClassDetailsWithLayout(class_id, room_id);
                                }, 1500);
                            }
                        }
                    });
                });
                $("#actions").show();
            }
            if ($(this).hasClass("undermaintenance")) {

            }
        }
    });

    $("#BookingSeatsTable a").click(function () {
        //first we check if action is of MoveToSpot or not
        if ($(this).hasClass("moveTo")) {
            $("body").attr("style","cursor:progress !important");
            $("#BookingSeatsTable").attr("disabled", "disabled").addClass("disabled").css({ "opacity": "0.5", "pointer-events": "none" });

            var classID = $("#BookingSeatsTable").data("classid");
            var roomID = $("#BookingSeatsTable").data("roomid");
            var spotNumber = $(this).data("spot");
            var client=$("spot.reserved.highlighted").find("client");
            var userID=$(client).data("userID");
                
            $("#BookingSeatsTable a").removeClass("moveTo");
        }
        else {
            $("#actions").hide();
            $("#BookingSeatsTable spot").removeClass("highlighted");
            $("#BookingSeatsTable a").removeClass("highlighted moveTo");

            $(this).addClass("highlighted");
            var spotNumber = $(this).data("spot");

            $("#actions").html('');
            $("#actions").append('<h2>Choose an action : </h2>');
            $("#actions").append('<div id="OptionGrid" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;"></div>');
            $("#OptionGrid").append('<button id="AssignUser" class="btn btn-danger" style="width:auto;margin-right:10px;" >Assign User to this Spot</button>');
            $("#OptionGrid").append('<button id="PutUnderMaintenance" class="btn btn-danger" style="width:auto;margin-right:10px;" >Put under maintenance</button>');

            $("#PutUnderMaintenance").click(function () {
                $("body").attr("style", "cursor:progress !important");
                $("#BookingSeatsTable").attr("disabled", "disabled").addClass("disabled").css({ "opacity": "0.5", "pointer-events": "none" });

                $("#SelectUserArea").remove();
                $("#actions").append('<div id="StatusMessageGrid" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;"></div>');
                var class_id = $("#BookingSeatsTable").data("classid");
                var room_id = $("#BookingSeatsTable").data("roomid");

                $.ajax({
                    url: '/admin/bookingOperation',
                    type: "post",
                    data: { operationID: 20, locationID: loc.locationID, classID: class_id, roomID: room_id, spotNumber: spotNumber },
                    success: function (data) {
                        if (data.statusCode == "Success") {
                            $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                            setTimeout(function () {
                                RenderClassDetailsWithLayout(class_id, room_id);
                            }, 1500);
                        }
                        if (data.statusCode == "Error") {
                            $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                            setTimeout(function () {
                                RenderClassDetailsWithLayout(class_id, room_id);
                            }, 1500);
                        }
                    }
                });
            });
            //show actions div at end

            $("#AssignUser").click(function () {
                $("#actions").append(
                    '<div id="SelectUserArea" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;">' +
                        '<div class="col-md-4">'+
                            '<select id="userList" style="margin-right:10px;padding-left:0px;width:100%;"></select>'+
                        '</div> '+
                        '<button id="AssignSpotToUser" class="btn btn-danger" class="width: auto;margin-right: 10px;height: 34px;">Assign</button>'+
                    '</div>'
                );

                $("#userList").select2();
                $("#userList").select2('destroy');
                $("#userList").select2({
                    ajax:
                    {
                        url: "admin/getMemberList",
                        dataType: 'json',
                        type: 'POST',
                        delay: 10,
                        data: function (param) {
                            return {
                                q: param.term // search term
                            };
                        },
                        processResults: function (data) {
                            // parse the results into the format expected by Select2.
                            // since we are using custom formatting functions we do not need to
                            // alter the remote JSON data
                            var res = [];
                            $.each(data, function (index, element) {
                                res.push({ id: element.userID, text: element.firstName + ' ' + element.lastName + ' ( ' + element.email + ' ) ' });
                            });
                            return {
                                results: res
                            };
                        },
                        cache: true,
                    },
                    placeholder: "Search member",
                    minimumInputLength: 1
                });

                $("#AssignSpotToUser").click(function () {
                    $("body").attr("style", "cursor:progress !important");
                    $("#BookingSeatsTable").attr("disabled", "disabled").addClass("disabled").css({ "opacity": "0.5", "pointer-events": "none" });
                    $("#actions").attr("disabled", "disabled").addClass("disabled").css({ "opacity": "0.5", "pointer-events": "none" });

                    $("#actions").append('<div id="StatusMessageGrid" class="row" style="padding:20px;padding-top:10px;display:flex;justify-content:center;"></div>');
                    var class_id = $("#BookingSeatsTable").data("classid");
                    var room_id = $("#BookingSeatsTable").data("roomid");
                    var user_id = $("#userList").val();

                    $.ajax({
                        url: '/admin/bookingOperation',
                        type: "post",
                        data: { operationID: 1, locationID: loc.locationID, classID: class_id, roomID: room_id, spotNumber: spotNumber,userID: user_id },
                        success: function (data) {
                            if (data.statusCode == "Success") {
                                $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                                setTimeout(function () {
                                    RenderClassDetailsWithLayout(class_id, room_id);
                                }, 1500);
                            }
                            if (data.statusCode == "Error") {
                                $("#StatusMessageGrid").append('<h3>' + data.message + '</h3>');
                                setTimeout(function () {
                                    RenderClassDetailsWithLayout(class_id, room_id);
                                }, 1500);
                            }
                        }
                    });
                });
            });

            $("#actions").show();
        }
    });
});