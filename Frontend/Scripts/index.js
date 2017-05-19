// request permission on page load
document.addEventListener('DOMContentLoaded', function () {
    if (!Notification) {
        alert('Desktop notifications not available in your browser. Try Chromium.');
        return;
    }

    if (Notification.permission !== "granted")
        Notification.requestPermission();
});

function notifyMe() {
    if (Notification.permission !== "granted")
        Notification.requestPermission();
    else {
        var notification = new Notification('Scrap The World', {
            icon: 'http://iconshow.me/media/images/Mixed/line-icon/png/256/world-256.png',
            body: "New results found!"
        });

        notification.onclick = function () {

        };
    }
}


var app = angular.module('MyApp', ['ngMaterial', 'SignalR'])
    .config(function ($mdThemingProvider) {

        $mdThemingProvider.theme('default')
		.primaryPalette('blue');

    })
    .controller('AppCtrl', ['$scope', '$http', '$window', '$mdDialog', 'HelloHubFactory', function ($scope, $http, $window, $mdDialog, HelloHubFactory) {

    var originatorEv;

    $scope.openMenu = function ($mdOpenMenu, ev) {
        originatorEv = ev;
        $mdOpenMenu(ev);
    };

	$scope.data = {
	    searchText: null,
	    notify: false
	};

	$scope.results = [];

    $scope.search = function () {
        HelloHubFactory.search($scope.data.searchText);
    }

    $scope.flow = function () {
        $window.open('/flow');
    }

    $scope.scripts = function () {
        $window.open('/scripts');
    }

    $scope.clear = function () {
        $scope.results = [];
    }

    $scope.getConnectionId = function() {
        return HelloHubFactory.getConnectionId();
    }

    $scope.$on('topic', function (event, arg) {
        $scope.results.unshift(arg);
        if ($scope.data.notify) {
            notifyMe();
        }
    });
}]);


app.directive('myEnter', function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if(event.which === 13) {
                    scope.$apply(function (){
                        scope.$eval(attrs.myEnter);
                    });

                    event.preventDefault();
                }
            });
        };
    })
    .factory('HelloHubFactory', ['$rootScope', 'Hub', function ($rootScope, Hub) {

    //declaring the hub connection
    var hub = new Hub('helloHub', {

        //client side methods
        listeners: {
            'addResult': function (data, image) {
                $rootScope.$broadcast('topic', { Data: data, Image: image, Time: new Date().toLocaleString() });
                $rootScope.$apply();
            }
        },

        //server side methods
        methods: ['search'],

        //query params sent on initial connection
        queryParams: {
            'token': 'exampletoken'
        },

        //handle connection error
        errorHandler: function (error) {
            console.error(error);
        },

        //specify a non default root
        //rootPath: '/api

        stateChanged: function (state) {
            switch (state.newState) {
                case $.signalR.connectionState.connecting:
                    break;
                case $.signalR.connectionState.connected:
                    break;
                case $.signalR.connectionState.reconnecting:
                    break;
                case $.signalR.connectionState.disconnected:
                    break;
            }
        }
    });

    var search = function (message) {
        hub.search(message);
    }

    var getConnectionId = function() {
        return hub.connection.id;
    }

    return {
        search: search,
        getConnectionId: getConnectionId
    };
}]);

