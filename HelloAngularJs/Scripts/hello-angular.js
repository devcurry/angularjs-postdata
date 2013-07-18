/// <reference path="jquery-2.0.3.js" />
/// <reference path="hello-angular.js" />
/// <reference path="angular.js" />

// Create ngTwitter Module (roughly Module = namespace in C#)
var ngTwitter = angular.module("ngTwitter", ['ngResource']);

// Declaring a Service
ngTwitter.factory("TwitterService", function ($resource)
{
    return {
        timeline: $resource("/Home/Tweet")
    }
});

ngTwitter.controller("TimelineController", function ($scope, TwitterService)
{
    $scope.tweets = TwitterService.timeline.query({}, isArray = true);

    $scope.newTweets = {
        0: "No new Tweets",
        other: "{} new Tweets"
    }

    $scope.sendStatus = function ()
    {
        var tweetText = $scope.statusText;
        var newTimeLine = new TwitterService.timeline(
            {
                Tweet: tweetText
            });
        newTimeLine.$save(function (data, headers)
        {
            if (data.success && data.success == true)
            {
                alert("Tweet Sent Successfully!");
                $scope.statusText = "";
            }
            else
            {
                alert("ERROR: " + data.errorMessage);
            }
        });
    }
});
