"use strict";
// Marco villegas 
var DateAndTime =
    {
        start: function () {

            var IDnames = new Array("FixEditFocus", "Description", "Descripcion", "UserName", "FirstName", "OldPassword");// använda för att kuna hitta ID i HTML systemet

            for (var i = 0; i < IDnames.length; i++) {

                var ID = document.getElementById(IDnames[i])// hittar ID i HTML

                //console.error(IDnames[i]);
                if (ID !== null && ID !== 'undefined')// kontrollerar att ID finns, om ID inte finns så körs if satsen som i sintur markerar en textruta för att underläta för användare 
                {
                    if (IDnames[i] === "FixEditFocus") {
                        document.getElementById("FirstName").focus();
                        break;
                    }
                    document.getElementById(IDnames[i]).focus();
                    break;
                }

            }

            // ändarar bakrund till fast vit färg som varar i 2 sceunder CSS klass 
           /* $('.container.body-content').hover(function () {
                $(this).attr('id', 'onabout');
              
                setTimeout(function () {
                    $('.container.body-content').attr('id', '');
                }, 20000);

            });*/





            if ($(window).width() < 900)// kontrollerar hur stor bildskärm man har
            {
                //kontrolerare om det fin text i en div med vis id 
                if ($('#Success1').text().trim().length) {
                    $('#Success1').attr('id', 'SuccessSmal');
                }


                var sec = 10;// används för att dölja meddelanden
                setTimeout(function () {
                    $("#SuccessSmal").hide();
                }, sec * 1000);

          //fixar menyn i elcetion view när det är mobil CSS
               
                $('.btn.btn-default').find('#knapptext').attr('id', 'knapptextV');
                $('.btn.btn-info').find('#knapptext').attr('id', 'knapptextB');
                //$('.btn.btn-info').find('#knapptextCandidate').attr('id', 'knapptextCandidateSmal');

                /* gör så texten i CSS mini knapparna bara blir 6 bokstäver*/
                /*var strB = $("#knapptextB").text();
               // $("#knapptextB").html(strB.substring(0, 6));
                var strV = $("#knapptextV").text();
                //$("#knapptextV").html(strV.substring(0, 6));

                $('.btn.btn-info').find('#knapptextB').html(strB.substring(0, 6));
                if (strV === 'Admin Off') {
                    $('.btn.btn-default').find('#knapptextV').html(strV.substring(0, 6));
                }*/

                //alert('Less than 960');
            }
            else {

                //fixar menyn i elcetion view när det är vanlig dator CSS
                /*$('.knapptextCandidateSmal').addClass('knapptextCandidate').removeClass('knapptextCandidateSmal');
                $('.btn.btn-default').find('.knapptextV').addClass('knapptext').removeClass('knapptextV');
                $('.btn.btn-info').find('.knapptextB').addClass('knapptext').removeClass('knapptextB');*/

                //kontrolerare om det fin text i en div med vis id 
                if ($('#Success1').text().trim().length) {
                    $('#Success1').attr('id', 'Success');
                }


                var sec = 10;// används för att dölja meddelanden
                setTimeout(function () {
                    $("#Success").hide();
                }, sec * 1000);

                //alert('More than 960');
            }



            if ($(window).width() < 900)// kontrollerar hur stor bildskärm man har
            {
                //hittar om en div med ett vist id har text inuti 
                if ($('#Error1').text().trim().length) {
                    $('#Error1').attr('id', 'ErrorSmal');
                }

                if ($('#Error2').text().trim().length) {
                    $('#Error2').attr('id', 'Error2Smal');
                }

                var sec = 10;// används för att dölja meddelanden
                setTimeout(function () {
                    $("#ErrorSmal").hide();
                }, sec * 1000);

                //alert('Less than 960');
            }
            else {
                //hittar om en div med ett vist id har text inuti 
                if ($('#Error1').text().trim().length) {
                    $('#Error1').attr('id', 'Error');
                }


                var sec = 10;// används för att dölja meddelanden
                setTimeout(function () {
                    $("#Error").hide();
                }, sec * 1000);

                //alert('More than 960');
            }



            //$('#TimeEnd').timepicker({ 'timeFormat': 'H:i' });
            $("input[type='time']").timepicker({ 'timeFormat': 'H:i' });// funktion som aktiverar TimePicker, den körs på alla input HTML tagar med typ (Time) 

            $("input[type='date']").datepicker({
                dateFormat: "yy-mm-dd",
            }); // funktion som aktiverar DatePicker, den körs på alla input HTML tagar med typ (Date) 

        },





    };

window.onload = DateAndTime.start;// startar funktionen som har label start när sidan har ladats