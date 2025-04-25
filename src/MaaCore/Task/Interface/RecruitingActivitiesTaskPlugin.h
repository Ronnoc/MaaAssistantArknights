#pragma once
#include "Task/AbstractTaskPlugin.h"
#include "InstHelper.h"

namespace asst
{
class RecruitingActivitiesPlugin : public AbstractTaskPlugin
{
public:
    using AbstractTaskPlugin::AbstractTaskPlugin;
    virtual ~RecruitingActivitiesPlugin() override = default;

    virtual bool verify(AsstMsg msg, const json::value& details) const override;

private:
    virtual bool _run() override;
    InstHelper m_inst_helper;
};
}
