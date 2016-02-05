﻿var abp = abp || {};
(function () {

    /* Application paths *****************************************/

    //Current application root path (including virtual directory if exists).
    abp.appPath = abp.appPath || '/';

    abp.pageLoadTime = new Date();

    //Converts given path to absolute path using abp.appPath variable.
    abp.toAbsAppPath = function (path) {
        if (path.indexOf('/') == 0) {
            path = path.substring(1);
        }

        return abp.appPath + path;
    };

    /* LOCALIZATION ***********************************************/
    //Implements Localization API that simplifies usage of localization scripts generated by Abp.

    abp.localization = abp.localization || {};

    abp.localization.localize = function (key, sourceName) {
        sourceName = sourceName || abp.localization.defaultSourceName;

        var source = abp.localization.values[sourceName];

        if (!source) {
            abp.log.warn('Could not find localization source: ' + sourceName);
            return key;
        }

        var value = source[key];
        if (value == undefined) {
            return key;
        }

        var copiedArguments = Array.prototype.slice.call(arguments, 0);
        copiedArguments.splice(1, 1);
        copiedArguments[0] = value;

        return abp.utils.formatString.apply(this, copiedArguments);
    };

    abp.localization.getSource = function (sourceName) {
        return function (key) {
            var copiedArguments = Array.prototype.slice.call(arguments, 0);
            copiedArguments.splice(1, 0, sourceName);
            return abp.localization.localize.apply(this, copiedArguments);
        };
    };

    abp.localization.isCurrentCulture = function (name) {
        return abp.localization.currentCulture
            && abp.localization.currentCulture.name
            && abp.localization.currentCulture.name.indexOf(name) == 0;
    };

    abp.localization.defaultSourceName = undefined;
    abp.localization.abpWeb = abp.localization.getSource('AbpWeb');

    /* AUTHORIZATION **********************************************/
    //Implements Authorization API that simplifies usage of authorization scripts generated by Abp.

    abp.auth = abp.auth || {};

    abp.auth.allPermissions = abp.auth.allPermissions || {};

    abp.auth.grantedPermissions = abp.auth.grantedPermissions || {};

    //Deprecated. Use abp.auth.isGranted instead.
    abp.auth.hasPermission = function (permissionName) {
        return abp.auth.isGranted.apply(this, arguments);
    };

    //Deprecated. Use abp.auth.isAnyGranted instead.
    abp.auth.hasAnyOfPermissions = function () {
        return abp.auth.isAnyGranted.apply(this, arguments);
    };

    //Deprecated. Use abp.auth.areAllGranted instead.
    abp.auth.hasAllOfPermissions = function () {
        return abp.auth.areAllGranted.apply(this, arguments);
    };

    abp.auth.isGranted = function (permissionName) {
        return abp.auth.allPermissions[permissionName] != undefined && abp.auth.grantedPermissions[permissionName] != undefined;
    };

    abp.auth.isAnyGranted = function () {
        if (!arguments || arguments.length <= 0) {
            return true;
        }

        for (var i = 0; i < arguments.length; i++) {
            if (abp.auth.isGranted(arguments[i])) {
                return true;
            }
        }

        return false;
    };

    abp.auth.areAllGranted = function () {
        if (!arguments || arguments.length <= 0) {
            return true;
        }

        for (var i = 0; i < arguments.length; i++) {
            if (!abp.auth.isGranted(arguments[i])) {
                return false;
            }
        }

        return true;
    };

    /* FEATURE SYSTEM *********************************************/
    //Implements Features API that simplifies usage of feature scripts generated by Abp.

    abp.features = abp.features || {};

    abp.features.allFeatures = abp.features.allFeatures || {};
    
    abp.features.get = function(name) {
        return abp.features.allFeatures[name];
    }

    abp.features.getValue = function (name) {
        var feature = abp.features.get(name);
        if (feature == undefined) {
            return undefined;
        }

        return feature.value;
    }
    
    abp.features.isEnabled = function (name) {
        var value = abp.features.getValue(name);
        return value == 'true' || value == 'True';
    }

    /* SETTINGS **************************************************/
    //Implements Settings API that simplifies usage of setting scripts generated by Abp.

    abp.setting = abp.setting || {};

    abp.setting.values = abp.setting.values || {};

    abp.setting.get = function (name) {
        return abp.setting.values[name];
    };

    abp.setting.getBoolean = function (name) {
        var value = abp.setting.get(name);
        return value == 'true' || value == 'True';
    };

    abp.setting.getInt = function (name) {
        return parseInt(abp.setting.values[name]);
    };

    /* REALTIME NOTIFICATIONS ************************************/

    abp.notifications = abp.notifications || {};
    abp.notifications.severity = {
        INFO: 0,
        SUCCESS: 1,
        WARN: 2,
        ERROR: 3,
        FATAL: 4
    };

    abp.notifications.userNotificationState = {
        UNREAD: 0,
        READ: 1
    };

    /* LOGGING ***************************************************/
    //Implements Logging API that provides secure & controlled usage of console.log

    abp.log = abp.log || {};

    abp.log.levels = {
        DEBUG: 1,
        INFO: 2,
        WARN: 3,
        ERROR: 4,
        FATAL: 5
    };

    abp.log.level = abp.log.levels.DEBUG;

    abp.log.log = function (logObject, logLevel) {
        if (!window.console || !window.console.log) {
            return;
        }

        if (logLevel != undefined && logLevel < abp.log.level) {
            return;
        }

        console.log(logObject);
    };

    abp.log.debug = function (logObject) {
        abp.log.log("DEBUG: ", abp.log.levels.DEBUG);
        abp.log.log(logObject, abp.log.levels.DEBUG);
    };

    abp.log.info = function (logObject) {
        abp.log.log("INFO: ", abp.log.levels.INFO);
        abp.log.log(logObject, abp.log.levels.INFO);
    };

    abp.log.warn = function (logObject) {
        abp.log.log("WARN: ", abp.log.levels.WARN);
        abp.log.log(logObject, abp.log.levels.WARN);
    };

    abp.log.error = function (logObject) {
        abp.log.log("ERROR: ", abp.log.levels.ERROR);
        abp.log.log(logObject, abp.log.levels.ERROR);
    };

    abp.log.fatal = function (logObject) {
        abp.log.log("FATAL: ", abp.log.levels.FATAL);
        abp.log.log(logObject, abp.log.levels.FATAL);
    };

    /* NOTIFICATION *********************************************/
    //Defines Notification API, not implements it

    abp.notify = abp.notify || {};

    abp.notify.success = function (message, title) {
        abp.log.warn('abp.notify.success is not implemented!');
    };

    abp.notify.info = function (message, title) {
        abp.log.warn('abp.notify.info is not implemented!');
    };

    abp.notify.warn = function (message, title) {
        abp.log.warn('abp.notify.warn is not implemented!');
    };

    abp.notify.error = function (message, title) {
        abp.log.warn('abp.notify.error is not implemented!');
    };

    /* MESSAGE **************************************************/
    //Defines Message API, not implements it

    abp.message = abp.message || {};

    var showMessage = function (message, title) {
        alert((title || '') + ' ' + message);

        if (!$) {
            abp.log.warn('abp.message can not return promise since jQuery is not defined!');
            return null;
        }

        return $.Deferred(function ($dfd) {
            $dfd.resolve();
        });
    };

    abp.message.info = function (message, title) {
        abp.log.warn('abp.message.info is not implemented!');
        return showMessage(message, title);
    };

    abp.message.success = function (message, title) {
        abp.log.warn('abp.message.success is not implemented!');
        return showMessage(message, title);
    };

    abp.message.warn = function (message, title) {
        abp.log.warn('abp.message.warn is not implemented!');
        return showMessage(message, title);
    };

    abp.message.error = function (message, title) {
        abp.log.warn('abp.message.error is not implemented!');
        return showMessage(message, title);
    };

    abp.message.confirm = function (message, titleOrCallback, callback) {
        abp.log.warn('abp.message.confirm is not implemented!');

        if (titleOrCallback && !(typeof titleOrCallback == 'string')) {
            callback = titleOrCallback;
        }

        var result = confirm(message);
        callback && callback(result);

        if (!$) {
            abp.log.warn('abp.message can not return promise since jQuery is not defined!');
            return null;
        }

        return $.Deferred(function ($dfd) {
            $dfd.resolve();
        });
    };

    /* UI *******************************************************/

    abp.ui = abp.ui || {};

    /* UI BLOCK */
    //Defines UI Block API, not implements it

    abp.ui.block = function (elm) {
        abp.log.warn('abp.ui.block is not implemented!');
    };

    abp.ui.unblock = function (elm) {
        abp.log.warn('abp.ui.unblock is not implemented!');
    };

    /* UI BUSY */
    //Defines UI Busy API, not implements it

    abp.ui.setBusy = function (elm, optionsOrPromise) {
        abp.log.warn('abp.ui.setBusy is not implemented!');
    };

    abp.ui.clearBusy = function (elm) {
        abp.log.warn('abp.ui.clearBusy is not implemented!');
    };

    /* SIMPLE EVENT BUS *****************************************/

    abp.event = (function() {

        var _callbacks = {};

        var on = function(eventName, callback) {
            if (!_callbacks[eventName]) {
                _callbacks[eventName] = [];
            }

            _callbacks[eventName].push(callback);
        };

        var trigger = function(eventName) {
            var callbacks = _callbacks[eventName];
            if (!callbacks || !callbacks.length) {
                return;
            }

            var args = Array.prototype.slice.call(arguments, 1);
            for (var i = 0; i < callbacks.length; i++) {
                callbacks[i].apply(this, args);
            }
        };

        // Public interface ///////////////////////////////////////////////////

        return {
            on: on,
            trigger: trigger
        };
    })();


    /* UTILS ***************************************************/

    abp.utils = abp.utils || {};

    /* Creates a name namespace.
    *  Example:
    *  var taskService = abp.utils.createNamespace(abp, 'services.task');
    *  taskService will be equal to abp.services.task
    *  first argument (root) must be defined first
    ************************************************************/
    abp.utils.createNamespace = function (root, ns) {
        var parts = ns.split('.');
        for (var i = 0; i < parts.length; i++) {
            if (typeof root[parts[i]] == 'undefined') {
                root[parts[i]] = {};
            }

            root = root[parts[i]];
        }

        return root;
    };

    /* Formats a string just like string.format in C#.
    *  Example:
    *  _formatString('Hello {0}','Halil') = 'Hello Halil'
    ************************************************************/
    abp.utils.formatString = function () {
        if (arguments.length < 1) {
            return null;
        }

        var str = arguments[0];

        for (var i = 1; i < arguments.length; i++) {
            var placeHolder = '{' + (i - 1) + '}';
            str = str.replace(placeHolder, arguments[i]);
        }

        return str;
    };

    abp.utils.toPascalCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toUpperCase();
        }

        return str.charAt(0).toUpperCase() + str.substr(1);
    }

    abp.utils.toCamelCase = function (str) {
        if (!str || !str.length) {
            return str;
        }

        if (str.length === 1) {
            return str.charAt(0).toLowerCase();
        }

        return str.charAt(0).toLowerCase() + str.substr(1);
    }

})();