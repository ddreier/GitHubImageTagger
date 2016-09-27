function Image(id, url) {
    var self = this;
    self.imageId = id;
    self.url = url;
    self.tags = ko.observableArray();
}

function SearchViewModel() {
    var self = this;

    self.images = ko.observableArray();

    self.populateImages = function (terms) {
        $.ajax("/api/tags/search", {
            type: "GET",
            data: { terms: terms },
            success: function (data, status, xhr) {
                $("#searchResult").html("Got back " + data.length + " results!");

                var mapped = $.map(data, function (item) {
                    return new Image(item.imageId, item.url);
                });
                self.images(mapped);

                self.populateImageTags();

                $("#searchSpinner").hide();
            },
            error: function (xhr, status, error) {
                $("#searchResult").html("Error " + status + " calling API: " + error);
                $("#searchSpinner").hide();
            }
        });
    }

    self.populateImageTags = function () {
        ko.utils.arrayForEach(self.images(), function (img) {
            $.getJSON("/api/images/tags/" + img.imageId,
                function (data) {
                    img.tags(data);
                }
            );
        });
    }
}

var viewModel = new SearchViewModel();
ko.applyBindings(viewModel);
