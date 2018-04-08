function RenderClassDetailsWithoutLayout(classID) {
    $("#initMsg").hide();
    $("#info").html('');
    $('#spots').html('');
    $("#actions").html('');
    $.ajax({
        url: '/admin/getClassReservationData',
        method: 'GET',
        data: { classID: classID, roomID: 0 },
        success: function (data) {
            var classInfo = data.classDetails;
            var reservationDetails = data.reservationDetails;
            $('div[data-classid="'+classID+'"][data-roomid="'+0+'"]').find('stat').text('('+reservationDetails.length+'/'+classInfo.classType.maxCapacity+')');
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
                '<option value="0">None</option>' +
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

            $("#spots").append('<div style="padding-left:30px;padding-right:30px;"><table class="table table-members" data-classid="' + classID + '" data-roomid="' + 0 + '" id="EnrolledUsersTable"><thead><tr><th>First Name</th><th>Last Name</th><th>Sign In</th><th>Cancel Reservation</th><th>View Profile</th></tr></thead><tbody></tbody></table></div>');
            $.each(reservationDetails, function (i, e) {
                var style = "";
                $("#EnrolledUsersTable tbody").append('<tr style="' + style + '"></tr>');
                $("#EnrolledUsersTable tbody tr:last-child").append('<td>' + e.user.firstName + '</td>');
                $("#EnrolledUsersTable tbody tr:last-child").append('<td>' + e.user.lastName + '</td>');

                if (e.signedIn == "1")
                    $("#EnrolledUsersTable tbody tr:last-child").append('<td><button data-resid="' + e.reservationID + '" style="width:auto;margin-right:10px;" class=\"checkOutBtn btn btn-danger\">Check - Out</button></td>');
                else
                    $("#EnrolledUsersTable tbody tr:last-child").append('<td><button data-resid="' + e.reservationID + '" style="width:auto;margin-right:10px;" class=\"checkInBtn btn btn-danger\">Check - In</button></td>');

                $("#EnrolledUsersTable tbody tr:last-child").append('<td><button data-userid="' + e.userID + '" id="CancelReservation' + e.userID + '" style="width:auto;margin-right:10px;" class=\"cancelResBtn btn btn-danger \">Cancel Member\'s Reservation</button></td>');

                $("#EnrolledUsersTable tbody tr:last-child").append('<td><button data-userid="' + e.userID + '"  style="width:auto;margin-right:10px;" class="ViewProfileBtn btn btn-danger">View Profile</button></td>');

            });
            $('#EnrolledUsersTable').DataTable({});
            $("#spots").show();
        }
    });
}