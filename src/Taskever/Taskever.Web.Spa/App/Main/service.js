﻿define(function () {
    return {
        load: function (name, req, onload, config) {

            //TODO: Test more
            //TODO: Make a better way of getting dto's
            var url;
            if (name == 'dto') {
                url = '/api/dtos';
            } else {
                url = '/api/ServiceProxies/?name=' + name;
            }
            
            req([url], function (value) {
                onload(value);
            });
        }
    };
});