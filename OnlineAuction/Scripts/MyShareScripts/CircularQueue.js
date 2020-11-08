class CircularQueueItem {
    constructor(value, next, back) {
        this.next = next;
        this.value = value;
        this.back = back;
        this.arr = []
    }
}
class CircularQueue {
    constructor(length) {
        this.length = length;
        //----------------------------------value,    next,   back --------------
        this.current = new CircularQueueItem(undefined, undefined, undefined);
        this.nextItem();
        this.arr = []
    }

    nextItem() {
        var item = this.current;
        for (var i = 0; i < this.length - 1; i++) {
            item.next = new CircularQueueItem(undefined, undefined, item);
            item = item.next;
        }
        item.next = this.current;
        this.current.back = item;
    }

    push(value) {
        this.current.value = value;
        this.current = this.current.next;
    }
    pop() {
        this.current = this.current.back;
        return this.current.value;
    }
    addRange(arr) {
        if (this.length < arr.length) {
            //увел. разм. массива+созд.нов.узлы + заполнение
            //this.length += arr.length - this.length;
            console.log("ERROR");
            return;
        }
        arr.forEach((i, val) => {
            this.push(i)
        })

    }
    //getCurrentArrValue() {  //это не фотограф. очереди!
    //    for (var i = 0; i < this.length; i++) {
    //        this.arr[i] = this.pop();
    //    }
    //    return this.arr;
    //}
    //getNextArrVal() {   //не работ.???????????????
    //    var res = this.pop();
    //    return this.getCurrentArrValue();
    //}
}