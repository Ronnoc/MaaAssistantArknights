#include "RecruitingActivitiesTaskPlugin.h"

#include "Controller/Controller.h"
#include "Utils/Logger.hpp"
#include "Vision/RegionOCRer.h"
#include "Config/TaskData.h"

bool asst::RecruitingActivitiesPlugin::verify(AsstMsg msg, const json::value& details) const
{
    if (msg != AsstMsg::SubTaskStart || details.get("subtask", std::string()) != "ProcessTask") {
        return false;
    }

    const std::string task = details.at("details").at("task").as_string();
    if (task == "RecruitingActivitiesTaskPlugin") {
        return true;
    }
    else {
        return false;
    }
}

bool asst::RecruitingActivitiesPlugin::_run()
{
    LogTraceFunction;

    const auto& task = Task.get<OcrTaskInfo>("RecruitingActivitiesTaskPlugin");

    cv::Mat image = m_inst_helper.ctrler()->get_image();

    RegionOCRer preproc_analyzer(image);
    preproc_analyzer.set_task_info(task);
    auto preproc_result_opt = preproc_analyzer.analyze();

    if (preproc_result_opt) {
        Log.error("OPERATOR FOUND: " + preproc_result_opt->text);
    }

    return true;
}
