function Image(id, url) {
    var self = this;
    self.imageId = id;
    self.url = url;
    self.tags = ko.observableArray();
    self.newTags = ko.observable();
    self.showNewTag = ko.observable(false);
    self.newTagStatusColor = ko.observable();

    self.deleteTag = function (tag) {
        console.debug(tag);
        $.ajax("/api/tags/" + tag.tagId, {
            type: "DELETE",
            success: function (data, status, xhr) {
                self.tags.remove(tag);
            },
            error: function (xhr, status, error) {
                alert("Error " + status + " calling API: " + error);
            }
        });
    };

    self.toggleShowNewTag = function () { self.showNewTag(!self.showNewTag()) };
}

//ko.bindingHandlers.addTagsPopover = {
//    init: function (element, valueAccessor, allBindingsAccessor, koViewModel, bindingContext) {
//        var options = ko.utils.unwrapObservable(valueAccessor());
//        var template = "<div class='input-group'><input type='text' class='form-control tagBox' placeholder='Enter tags, comma separated.' /><span class='input-group-btn'><button class='btn btn-default' type='button' data-bind='click: alert('hi')'><i class='fa fa-check'></i></button></span></div>";
//        var defaultOptions = { html: true, content: template };
//        options = $.extend(true, {}, defaultOptions, options);
//        var pop = $(element).popover(options);
//        ko.applyBindingsToDescendants(bindingContext, pop);
//    }
//}

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
    };

    self.populateImageTags = function () {
        ko.utils.arrayForEach(self.images(), function (img) {
            $.getJSON("/api/images/tags/" + img.imageId,
                function (data) {
                    img.tags(data);
                }
            );
        });
    };

    self.populateTagsForImage = function (img) {
        $.getJSON("/api/images/tags/" + img.imageId,
                function (data) {
                    img.tags(data);
                }
            );
    };

    self.submitTags = function (img) {
        //img.editEnabled(false);
        var data = { tags: img.newTags(), imageId: img.imageId };

        if (data.tags !== "") {
            $.ajax("/api/tags", {
                type: "POST",
                data: data,
                success: function (data, status, xhr) {
                    img.newTagStatusColor("limegreen");
                    self.populateTagsForImage(img);
                    img.newTags(null);
                    img.showNewTag(false);
                },
                error: function (xhr, status, error) {
                    console.log("error submitting tags: " + error);
                    img.newTagStatusColor("red");
                }
            });
        }
    };
}

var viewModel = new SearchViewModel();
ko.applyBindings(viewModel);
