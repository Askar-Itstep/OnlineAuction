function getDateTime(parametr) {
    let day = parametr.split(' ')[0].split('.')[0];
    let month = parametr.split(' ')[0].split('.')[1];
    let year = parametr.split(' ')[0].split('.')[2];
    let time = parametr.split(" ")[1];
    time = checkHour(time)
    //1, 2 - в контроллер, 3- для расчета длительн.
    return [new Object(year + '-' + month + '-' + day + 'T' + time), time,
    new Date(year + '-' + month + '-' + day + 'T' + time)]
}

//в БД время может хран. в формате 1:15:00, от чего у JS запор
function checkHour(time) {
    let hour = time.split(':')[0]
    hour = hour.length == 1 ? "0" + hour : hour;
    return hour + ':' + time.split(':')[1] + ':' + time.split(':')[2];
}