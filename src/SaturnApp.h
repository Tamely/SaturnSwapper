#pragma once
#include <AppCore/AppCore.h>

using namespace ultralight;

class SaturnApp : public AppListener,
              public WindowListener,
              public LoadListener,
              public ViewListener {
public:
    SaturnApp();

  virtual ~SaturnApp();

  // Start the run loop.
  virtual void Run();

  // This is called continuously from the app's main loop.
  virtual void OnUpdate() override;

  // This is called when the window is closing.
  virtual void OnClose(ultralight::Window* window) override;

  // This is called whenever the window resizes.
  virtual void OnResize(ultralight::Window* window, uint32_t width, uint32_t height) override;

  // This is called when the page finishes a load in one of its frames.
  virtual void OnFinishLoading(ultralight::View* caller,
                               uint64_t frame_id,
                               bool is_main_frame,
                               const String& url) override;

  // This is called when the DOM has loaded in one of its frames.
  virtual void OnDOMReady(ultralight::View* caller,
                          uint64_t frame_id,
                          bool is_main_frame,
                          const String& url) override;

  // This is called when the page requests to change the Cursor.
  virtual void OnChangeCursor(ultralight::View* caller,
    Cursor cursor) override;

  virtual void OnChangeTitle(ultralight::View* caller,
    const String& title) override;

protected:
  RefPtr<App> app_;
  RefPtr<Window> window_;
  RefPtr<Overlay> overlay_;
};
