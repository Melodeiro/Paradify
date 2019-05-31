(function () {
    var defaults = {
        playPauseClass: '.play-pause'
    }
    $(document).ready(function () {
        initPlayback();
    });

    this.initPlayback = function () {
        initImageMouseOver();
        $(".number-playback-pic").each(function () {
            var url = $(this).attr("playback");
            var trackName = $(this).attr("trackName");

            var first = $(this).find(defaults.playPauseClass)[0];
            if (url != undefined && url != '') {

                initClick(first, url, trackName);
                //initHover(first);
            }
            else {
                $(first).removeClass('fa-play');
            }
        });
    }

    var setBackgroud = function (elem, className) {
        $(elem).addClass(className);
    }

    var initClick = function (elem, preview_url, trackName) {
        $(elem).unbind("click");

        $(elem).click(function () {

            if ($(elem).hasClass('fa-pause')) {
                pause(preview_url);
                $(elem).removeClass('fa-pause');
                $(elem).addClass('fa-play');
            }
            else {
                play(preview_url);
                $(elem).removeClass('fa-play');
                $(elem).addClass('fa-pause');
                gaEvent.track.play(trackName);
            }
        });
    }

    var initHover = function (elem) {
        
        $(elem).mouseover(function () {
            if ($(elem).hasClass('play') == false) {
                $(elem).addClass('play');
            }

            $(elem).css('background-color', 'black');
            //$(elem).fadeTo("fast", 1, function () {

            //});
        });

        $(elem).mouseout(function () {
            $(elem).css('background-color', 'none');
            //$(elem).fadeTo("fast", 0, function () {
            //});
        });
    }

    var initImageMouseOver = function() {
        $(".track-item").each(function () {

            $(this).mouseover(function () {
                $(this).find('.track-item-overlay').show();
            });

            $(this).mouseout(function () {
                $(this).find('.track-item-overlay').hide();
            });

        });
    }

     
}());

var p;
function play(url) {

    if (p != null) {
        if (p.src != url) {
            p.src = url;
        }
    } else {
        p = new Audio();
        p.src = url;

    }

    p.play();
}


function pause(url) {
    if (p != null && p.src == url) {
        p.pause();
    }
}