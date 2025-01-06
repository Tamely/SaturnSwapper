#include "SaturnApp.h"

#include <string>

#define WINDOW_WIDTH  1152
#define WINDOW_HEIGHT 648

import Saturn.Context;

import Saturn.Items.ItemModel;

import Saturn.SaturnFunctionLibrary;
import Saturn.FortniteFunctionLibrary;

import Saturn.CallbackFunctions.OnSearch;
import Saturn.CallbackFunctions.OnGetKey;
import Saturn.CallbackFunctions.OnGetFOV;
import Saturn.CallbackFunctions.OnSetFOV;
import Saturn.CallbackFunctions.OnOpenKey;
import Saturn.CallbackFunctions.OnAddItem;
import Saturn.CallbackFunctions.OnCheckKey;
import Saturn.CallbackFunctions.OnResetTab;
import Saturn.CallbackFunctions.OnNextPage;
import Saturn.CallbackFunctions.OnAddLoadout;
import Saturn.CallbackFunctions.OnLoadSaturn;
import Saturn.CallbackFunctions.OnCloseSaturn;
import Saturn.CallbackFunctions.OnLaunchClick;
import Saturn.CallbackFunctions.OnApplyLoadout;
import Saturn.CallbackFunctions.OnLoadLoadouts;
import Saturn.CallbackFunctions.OnPreviousPage;
import Saturn.CallbackFunctions.OnDisplayItems;
import Saturn.CallbackFunctions.OnSelectPlugin;
import Saturn.CallbackFunctions.OnGenerateSkins;
import Saturn.CallbackFunctions.OnLoadPluginTab;
import Saturn.CallbackFunctions.OnDownloadPlugin;
import Saturn.CallbackFunctions.OnRunLocalPlugin;
import Saturn.CallbackFunctions.OnRevertAndClose;
import Saturn.CallbackFunctions.OnDisplayPlugins;
import Saturn.CallbackFunctions.OnGenerateEmotes;
import Saturn.CallbackFunctions.OnIsItemConverted;
import Saturn.CallbackFunctions.OnGeneratePickaxes;
import Saturn.CallbackFunctions.OnGenerateBackblings;

import <vector>;

SaturnApp::SaturnApp() {
  app_ = App::Create();

  window_ = Window::Create(app_->main_monitor(), WINDOW_WIDTH, WINDOW_HEIGHT,
    false, kWindowFlags_Titled);

  overlay_ = Overlay::Create(window_, 1, 1, 0, 0);

  OnResize(window_.get(), window_->width(), window_->height());

  overlay_->view()->LoadURL("file:///app.html");
  app_->set_listener(this);
  window_->set_listener(this);
  overlay_->view()->set_load_listener(this);
  overlay_->view()->set_view_listener(this);
}

SaturnApp::~SaturnApp() {
}

void SaturnApp::Run() {
    app_->Run();
}

void SaturnApp::OnUpdate() {
    // Update loop
}

void SaturnApp::OnClose(ultralight::Window* window) {
  app_->Quit();
}

void SaturnApp::OnResize(ultralight::Window* window, uint32_t width, uint32_t height) {
  overlay_->Resize(width, height);
}

void SaturnApp::OnFinishLoading(ultralight::View* caller,
                                uint64_t frame_id,
                                bool is_main_frame,
                                const String& url) {
}

void SaturnApp::OnDOMReady(ultralight::View* caller,
                           uint64_t frame_id,
                           bool is_main_frame,
                           const String& url) {
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnSearch::GetName(), FOnSearch::OnSearch);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGetKey::GetName(), FOnGetKey::OnGetKey);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGetFOV::GetName(), FOnGetFOV::OnGetFOV);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnSetFOV::GetName(), FOnSetFOV::OnSetFOV);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnOpenKey::GetName(), FOnOpenKey::OnOpenKey);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnAddItem::GetName(), FOnAddItem::OnAddItem);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnCheckKey::GetName(), FOnCheckKey::OnCheckKey);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnResetTab::GetName(), FOnResetTab::OnResetTab);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnNextPage::GetName(), FOnNextPage::OnNextPage);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnAddLoadout::GetName(), FOnAddLoadout::OnAddLoadout);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnLoadSaturn::GetName(), FOnLoadSaturn::OnLoadSaturn);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnCloseSaturn::GetName(), FOnCloseSaturn::OnCloseSaturn);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnLaunchClick::GetName(), FOnLaunchClick::OnLaunchClick);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnApplyLoadout::GetName(), FOnApplyLoadout::OnApplyLoadout);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnLoadLoadouts::GetName(), FOnLoadLoadouts::OnLoadLoadouts); 
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnPreviousPage::GetName(), FOnPreviousPage::OnPreviousPage);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnDisplayItems::GetName(), FOnDisplayItems::OnDisplayItems);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnSelectPlugin::GetName(), FOnSelectPlugin::OnSelectPlugin);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnLoadPluginTab::GetName(), FOnLoadPluginTab::OnLoadPluginTab);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGenerateSkins::GetName(), FOnGenerateSkins::OnGenerateSkins); 
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnDownloadPlugin::GetName(), FOnDownloadPlugin::OnDownloadPlugin);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnRunLocalPlugin::GetName(), FOnRunLocalPlugin::OnRunLocalPlugin);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnRevertAndClose::GetName(), FOnRevertAndClose::OnRevertAndClose);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnDisplayPlugins::GetName(), FOnDisplayPlugins::OnDisplayPlugins);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGenerateEmotes::GetName(), FOnGenerateEmotes::OnGenerateEmotes); 
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnIsItemConverted::GetName(), FOnIsItemConverted::OnIsItemConverted); 
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGeneratePickaxes::GetName(), FOnGeneratePickaxes::OnGeneratePickaxes);
    SaturnFunctionLibrary::BIND_CALLBACK(caller, FOnGenerateBackblings::GetName(), FOnGenerateBackblings::OnGenerateBackblings);
}

void SaturnApp::OnChangeCursor(ultralight::View* caller,
                               Cursor cursor) {
  window_->SetCursor(cursor);
}

void SaturnApp::OnChangeTitle(ultralight::View* caller,
                              const String& title) {
  window_->SetTitle(title.utf8().data());
}
