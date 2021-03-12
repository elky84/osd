function on_init(me)
    print('init')
end

function on_finish(me)
    print('finish')
end

function on_tick(me)
    me:hp_add(-10, me)
end