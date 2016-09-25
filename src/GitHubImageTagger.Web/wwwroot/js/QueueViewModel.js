function Image(id, url, tags) {
    var self = this;
    self.imageId = id;
    self.url = url;
    self.tags = ko.observable(tags);
    self.editEnabled = ko.observable(true);
    self.updateStatusColor = ko.observable();
}

function QueueViewModel() {
    var self = this;

    self.images = ko.observableArray();

    self.submitTags = function (img) {
        img.editEnabled(false);
        var data = { tags: img.tags, imageId: img.imageId };

        if (data.tags !== "") {
            $.ajax("/api/tags", {
                type: "POST",
                data: data,
                success: function (data, status, xhr) {
                    img.updateStatusColor("limegreen");
                },
                error: function (xhr, status, error) {
                    console.log("error submitting tags: " + error);
                    img.updateStatusColor("red");
                }
            });
        }
    };

    $.getJSON("/api/images/untagged/take/15", function (allData) {
        var mapped = $.map(allData, function (item) { return new Image(item.imageId, item.url, ""); });
        self.images(mapped);
    });
}

var viewModel = new QueueViewModel()
ko.applyBindings(viewModel);