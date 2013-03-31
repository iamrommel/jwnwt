function BookNameViewModel() {
    var self = this;

    self.BookIndex = function(language) {
        if (language == 'tg')
            return BookIndex.tgIndex;

        return null;
    };
    

    //The booknames based on the value return from Book Index the will set the language
    self.bookNames = ko.observableArray();

    //calls this method before other operation
    self.initialize = function () {
        
        //this will get the book names betting filtering the value of chapter as 1
        var bookNames = Enumerable.From(self.BookIndex('tg'))
            .Where(function (item) {
                return item.cp == 1;
            })
            .Select(function (item) {
                return item.n;
            }).ToArray();

        //clear the array before pushing the value
        self.bookNames.removeAll();
        jQuery.each(bookNames, function (index, value) {
            self.bookNames.push(value);
        });

        
    };

}