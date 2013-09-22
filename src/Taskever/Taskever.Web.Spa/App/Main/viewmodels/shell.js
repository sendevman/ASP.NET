﻿define(['plugins/router', 'durandal/app'], function (router, app) {
    return {
        router: router,
        activate: function () {
            router.map([
                { route: '', title: 'Home', moduleId: 'viewmodels/home', nav: true },
                { route: 'user', title: 'My Profile', moduleId: 'viewmodels/user', nav: true },
                { route: 'friends', title: 'My Friends', moduleId: 'viewmodels/friends', nav: true }
            ]).buildNavigationModel();

            return router.activate();
        }
    };
});