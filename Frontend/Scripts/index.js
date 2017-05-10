
var app = angular.module('MyApp', ['ngMaterial', 'SignalR'])
    .config(function ($mdThemingProvider) {

        $mdThemingProvider.theme('default')
		.primaryPalette('blue');

    })
    .controller('AppCtrl', ['$scope', '$http', 'HelloHubFactory', function ($scope, $http, HelloHubFactory) {

    $scope.searchText = null;
    $scope.results = [];

    $scope.search = function () {
        HelloHubFactory.search($scope.searchText);
    }

    $scope.getConnectionId = function() {
        return HelloHubFactory.getConnectionId();
    }

    $scope.$on('topic', function (event, arg) {
        $scope.results.unshift(arg);
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

