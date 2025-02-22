$(function () {
    
    Ladda.bind('p:not(.ladda-progress-demo) button.ladda-button', {
        timeout: 2000
    });
    Ladda.bind('a.ladda-button', {
        timeout: 250000
    });
    // Bind progress buttons and simulate loading progress
    Ladda.bind('.ladda-progress-demo button.ladda-button', {
        callback: function (instance) {
            var progress = 0;
            var interval = setInterval(function () {
                progress = Math.min(progress + Math.random() * 0.1, 20);
                instance.setProgress(progress);

                if (progress === 1) {
                    instance.stop();
                    clearInterval(interval);
                }
            }, 200);
        }
    });

});