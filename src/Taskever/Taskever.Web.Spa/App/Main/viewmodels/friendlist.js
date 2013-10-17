﻿define(["jquery", "knockout", 'durandal/app', 'plugins/dialog', 'service!taskever/friendship'], function ($, ko, app, dialogs, friendshipService) {
    var friends = ko.mapping.fromJS([]);

    return {
        friends: friends,

        activate: function () {
            friendshipService.getMyFriends({})
                .then(function (data) {
                    ko.mapping.fromJS(data, friends);
                });
        }
    };
});