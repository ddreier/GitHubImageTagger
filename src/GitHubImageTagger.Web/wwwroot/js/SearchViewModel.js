function Image(id, url) {
    var self = this;
    self.imageId = id;
    self.url = url;
    self.tags = ko.observableArray();
    self.newTags = ko.observable();
    self.showNewTag = ko.observable(false);
    self.newTagStatusColor = ko.observable();
    self.editingTag = ko.observable({ content: '' });
    self.showEditTag = ko.observable(false);
    self.editTagStatusColor = ko.observable();

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

    self.editTag = function () {
        $.ajax("/api/tags/" + self.editingTag().tagId, {
            method: "PUT",
            data: JSON.stringify(self.editingTag().content),
            contentType: "application/json",
            success: function (img) {
                self.showEditTag(false);
                self.populateTags();
            }
        });
    }

    self.editTagKeyUp = function (data, event) {
        //console.debug(event);
        if (event.keyCode === 13) {
            self.editTag();
        }
    }

    self.populateTags = function () {
        $.getJSON("/api/images/tags/" + self.imageId,
                function (data) {
                    self.tags(data);
                }
            );
    }

    self.toggleShowNewTag = function () { self.showNewTag(!self.showNewTag()) };

    self.toggleShowEditTag = function (tag) {
        self.showEditTag(true);
        self.editingTag(tag);
    }
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

    self.newTagKeyUp = function (data, event) {
        if (event.keyCode === 13) {
            self.submitTags(data);
        }
    }
}

var viewModel = new SearchViewModel();
ko.applyBindings(viewModel);
