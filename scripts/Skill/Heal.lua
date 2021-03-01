function func(me)
    local map = me:map()
    local mobs = me:nears(1)

    for _, mob in pairs(mobs) do
        local base_hp = mob:base_hp()

        -- 최대 체력의 10%만큼 체력 회복
        mob:hp_add(base_hp * 0.1, me)
    end
end