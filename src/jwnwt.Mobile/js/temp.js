//function loadBookIndex(languageName) {
//    $.getJSON("/js/" + languageName + "-bookindex.js",
//         function (response) {

//             bookIndex = response;

//             //this will get the book names betting filtering the value of chapter as 1
//             var bookNames = Enumerable.From(bookIndex)
//                 .Where(function (item) {
//                     return item.cp == 1;
//                 })
//                 .Select(function (item) {
//                     return item.n;
//                 }).ToArray();




//             jQuery.each(bookNames, function(index, value) {
//                 $("#bookNames").append("<li>" + value + "</li>");
//             });



//         });
//}



//function onSearchBibleText() {
//    $("#injection").load("template/tg/05_BI12_.GE-split11.xhtml", function () {

//        highligthText("#chapter11_verse20");
//        highligthText("#chapter11_verse21");
//        highligthText("#chapter11_verse22");

//        $('html, body').animate({ scrollTop: $('#chapter11_verse20').offset().top }, 'normal');

//    });
//}


//function highligthText(id) {


//    //this will select all those text in the paragraph and createe span
//    $(id).parent().contents().filter(function () {
//        return this.nodeType == 3;
//    })
//        .wrap('<span></span>')
//        .end();

//    $(id).parent().find('span')
//       .nextUntil("a")
//       .toggleClass('selected')
//    ;

//}